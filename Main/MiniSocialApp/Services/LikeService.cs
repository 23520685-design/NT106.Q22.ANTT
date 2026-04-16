using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class LikeService
    {
        private readonly FirestoreDb _db;

        public LikeService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task<object> ToggleLike(string postId)
        {
            var userDict = CurrentUserStore.User as Dictionary<string, object>;
            string userId = userDict != null && userDict.ContainsKey("userId")
                ? userDict["userId"]?.ToString()
                : "";

            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not logged in");

            var likeRef = _db.Collection("posts")
                .Document(postId)
                .Collection("likes")
                .Document(userId);

            var postRef = _db.Collection("posts").Document(postId);

            bool liked = false;

            await _db.RunTransactionAsync(async transaction =>
            {
                var likeSnap = await transaction.GetSnapshotAsync(likeRef);

                if (likeSnap.Exists)
                {
                    transaction.Delete(likeRef);
                    transaction.Update(postRef, new Dictionary<string, object>
            {
                { "likeCount", FieldValue.Increment(-1) }
            });
                    liked = false;
                }
                else
                {
                    transaction.Set(likeRef, new Dictionary<string, object>
            {
                { "userId", userId },
                { "likedAt", Timestamp.GetCurrentTimestamp() }
            });

                    transaction.Update(postRef, new Dictionary<string, object>
            {
                { "likeCount", FieldValue.Increment(1) }
            });
                    liked = true;
                }
            });

            var postSnap = await postRef.GetSnapshotAsync();
            int likeCount = 0;

            if (postSnap.Exists && postSnap.ContainsField("likeCount"))
            {
                likeCount = postSnap.GetValue<int>("likeCount");
            }

            return new
            {
                postId = postId,
                liked = liked,
                likeCount = likeCount
            };
        }
    }
}
