using Google.Cloud.Firestore;
using MiniSocialApp.Config;
using System;

public class FirestoreContext
{
    public FirestoreDb Db { get; private set; }
    // ✅ FIX 8: Xóa static DB property dead code – không dùng đến

    public FirestoreContext()
    {
        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            FirebaseConfig.CredentialPath
        );

        Db = FirestoreDb.Create(FirebaseConfig.ProjectId);
    }
}