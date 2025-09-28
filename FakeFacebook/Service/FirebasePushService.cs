using FakeFacebook.Data;
using FakeFacebook.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using System;

namespace FakeFacebook.Service
{
    public class FirebasePushService : IFirebasePushService
    {
        private readonly FakeFacebookDbContext _db;

        public FirebasePushService(FakeFacebookDbContext db)
        {
            _db = db;

            // Khởi tạo FirebaseApp duy nhất
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                });
            }
        }

        /// <summary>
        /// Đăng ký hoặc cập nhật FCM token cho user
        /// </summary>
        public async Task RegisterTokenAsync(string userId, string token)
        {
            var existing = _db.UserTokens.FirstOrDefault(x => x.UserId == userId);
            if (existing == null)
            {
                _db.UserTokens.Add(new UserToken { UserId = userId, Token = token });
            }
            else
            {
                existing.Token = token;
                _db.UserTokens.Update(existing);
            }
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Gửi notification tới user theo userId
        /// </summary>
        public async Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string> data)
        {
            var token = _db.UserTokens.FirstOrDefault(x => x.UserId == userId)?.Token;
            if (string.IsNullOrEmpty(token))
                throw new Exception($"User {userId} chưa có FCM token");

            var message = new Message
            {
                Token = token,
                Notification = new Notification { Title = title, Body = body },
                Data = data ?? new Dictionary<string, string>()
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
