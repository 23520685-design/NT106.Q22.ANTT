using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class FirebaseAuthUserService
    {
        private readonly FirestoreDb _db;

        public FirebaseAuthUserService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task<Dictionary<string, object>> LoginOrCreateFirebaseUser(
            string firebaseUid,
            string email,
            string displayName,
            string photoUrl = ""
        )
        {
            if (string.IsNullOrWhiteSpace(firebaseUid))
                throw new Exception("Firebase UID không hợp lệ.");

            email = email != null ? email.Trim() : "";
            displayName = displayName != null ? displayName.Trim() : "";
            photoUrl = photoUrl != null ? photoUrl.Trim() : "";

            DocumentReference userRef = _db.Collection("users").Document(firebaseUid);
            DocumentSnapshot userDoc = await userRef.GetSnapshotAsync();

            if (userDoc.Exists)
            {
                var existingUser = userDoc.ToDictionary();

                if (!existingUser.ContainsKey("userId"))
                    existingUser["userId"] = firebaseUid;

                var updateData = new Dictionary<string, object>
                {
                    { "authProvider", "firebase" },
                    { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
                };

                if (!string.IsNullOrWhiteSpace(email))
                {
                    existingUser["email"] = email;
                    updateData["email"] = email;
                }

                if (!existingUser.ContainsKey("phone"))
                {
                    existingUser["phone"] = "";
                    updateData["phone"] = "";
                }

                if (!existingUser.ContainsKey("bio"))
                {
                    existingUser["bio"] = "";
                    updateData["bio"] = "";
                }

                if (!existingUser.ContainsKey("avatar") && !string.IsNullOrWhiteSpace(photoUrl))
                {
                    existingUser["avatar"] = photoUrl;
                    updateData["avatar"] = photoUrl;
                }

                await userRef.SetAsync(updateData, SetOptions.MergeAll);
                return existingUser;
            }

            string userName = !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : !string.IsNullOrWhiteSpace(email)
                    ? email.Split('@')[0]
                    : "User";

            string avatar = !string.IsNullOrWhiteSpace(photoUrl)
                ? photoUrl
                : "https://i.pravatar.cc/150?u=" + firebaseUid;

            var newUser = new Dictionary<string, object>
            {
                { "userId", firebaseUid },
                { "userName", userName },
                { "email", email },
                { "phone", "" },
                { "avatar", avatar },
                { "bio", "" },
                { "followersCount", 0 },
                { "followingCount", 0 },
                { "postCount", 0 },
                { "authProvider", "firebase" },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
            };

            await userRef.SetAsync(newUser);
            return newUser;
        }
    }
}