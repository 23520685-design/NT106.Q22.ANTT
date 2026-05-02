using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class CommentService
    {
        private readonly FirestoreDb _db;

        public CommentService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task<Dictionary<string, object>> CreateComment(string postId, string content)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new Exception("PostId khong hop le.");

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("Binh luan khong duoc de trong.");

            var userDict = CurrentUserStore.User as Dictionary<string, object>;
            if (userDict == null)
                throw new Exception("Nguoi dung chua dang nhap.");

            string userId = userDict.ContainsKey("userId") ? Convert.ToString(userDict["userId"]) : null;
            string userName = userDict.ContainsKey("userName") ? Convert.ToString(userDict["userName"]) : null;
            string avatar = userDict.ContainsKey("avatar") ? Convert.ToString(userDict["avatar"]) : "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userName))
                throw new Exception("Thong tin nguoi dung khong hop le.");

            var postRef = _db.Collection("posts").Document(postId);
            var commentRef = postRef.Collection("comments").Document();
            var createdAt = Timestamp.GetCurrentTimestamp();

            var comment = new Dictionary<string, object>
            {
                { "commentId", commentRef.Id },
                { "postId", postId },
                { "content", content.Trim() },
                { "userId", userId },
                { "userName", userName },
                { "avatar", avatar },
                { "createdAt", createdAt }
            };

            await _db.RunTransactionAsync(async transaction =>
            {
                var postSnap = await transaction.GetSnapshotAsync(postRef);
                if (!postSnap.Exists)
                    throw new Exception("Bai viet khong ton tai.");

                transaction.Set(commentRef, comment);
                transaction.Update(postRef, new Dictionary<string, object>
                {
                    { "commentCount", FieldValue.Increment(1) }
                });
            });

            var updatedPost = await postRef.GetSnapshotAsync();
            int commentCount = 0;
            if (updatedPost.Exists && updatedPost.ContainsField("commentCount"))
                commentCount = updatedPost.GetValue<int>("commentCount");

            comment["commentCount"] = commentCount;
            comment["createdAt"] = createdAt.ToDateTime().ToUniversalTime();

            return comment;
        }

        public async Task<List<Dictionary<string, object>>> GetComments(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new Exception("PostId khong hop le.");

            var snapshot = await _db.Collection("posts")
                .Document(postId)
                .Collection("comments")
                .OrderBy("createdAt")
                .Limit(100)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(doc =>
            {
                var data = doc.ToDictionary();
                data["commentId"] = doc.Id;
                data["postId"] = postId;

                if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
                    data["createdAt"] = ts.ToDateTime().ToUniversalTime();

                return data;
            }).ToList();
        }
    }
}
