using System.Security.AccessControl;
using System.Security.Principal;

namespace Hub;

/// <summary>
/// Yükseltilmiş yetkiyle çalıştırılacak dosyaların (GoodbyeDPI hizmeti SYSTEM olarak, arayüz en yüksek
/// yetkili görevle) yönetici olmayan biri tarafından değiştirilemeyeceğini denetler. Aksi halde o kişi
/// dosyayı değiştirip yönetici/SYSTEM yetkisi kazanabilir.
/// </summary>
static class Acl
{
    // Yazma/dosya oluşturma, silme, izin ya da sahiplik değiştirme; ham GENERIC_WRITE ve GENERIC_ALL dahil.
    const int UnsafeRights = (int)(FileSystemRights.Write | FileSystemRights.Delete |
        FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.ChangePermissions |
        FileSystemRights.TakeOwnership) | 0x40000000 | 0x10000000;

    static readonly SecurityIdentifier[] Untrusted =
    [
        new(WellKnownSidType.WorldSid, null),
        new(WellKnownSidType.AuthenticatedUserSid, null),
        new(WellKnownSidType.BuiltinUsersSid, null),
        new(WellKnownSidType.InteractiveSid, null),
        // Aynı hesabın yükseltilmemiş süreçleri de bu kimlikle çalışır.
        WindowsIdentity.GetCurrent().User!,
    ];

    /// <summary>Verilen klasör ve dosyaların hiçbiri yönetici olmayanlarca değiştirilemiyorsa true.</summary>
    public static bool AdminOnly(IEnumerable<string> paths)
    {
        try { return paths.All(AdminOnly); }
        catch { return false; } // okunamıyorsa güvenli sayma
    }

    static bool AdminOnly(string path)
    {
        FileSystemSecurity sec = Directory.Exists(path)
            ? new DirectoryInfo(path).GetAccessControl()
            : new FileInfo(path).GetAccessControl();

        // Sahip, izinleri her zaman değiştirebilir.
        if (IsUntrusted(sec.GetOwner(typeof(SecurityIdentifier)))) return false;

        foreach (FileSystemAccessRule r in sec.GetAccessRules(true, true, typeof(SecurityIdentifier)))
        {
            if (r.AccessControlType != AccessControlType.Allow) continue;
            if ((r.PropagationFlags & PropagationFlags.InheritOnly) != 0) continue; // sadece alt öğelere uygulanır
            if (((int)r.FileSystemRights & UnsafeRights) == 0) continue;
            if (IsUntrusted(r.IdentityReference)) return false;
        }
        return true;
    }

    static bool IsUntrusted(IdentityReference? id) => id is SecurityIdentifier sid && Untrusted.Contains(sid);
}
