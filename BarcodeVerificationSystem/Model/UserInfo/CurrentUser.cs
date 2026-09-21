using BarcodeVerificationSystem.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeVerificationSystem.Model.UserInfo
{
    public class CurrentUser
    {
        public static string UserName
        {
            get
            {
                try
                {
                    // Ưu tiên lấy từ OnlineUserModel
                    if (Shared.UserPermission?.OnlineUserModel != null)
                    {
                        return Shared.UserPermission.OnlineUserModel.ten_tai_khoan;
                    }

                    // Nếu không có, lấy từ LoggedInUser và giải mã
                    if (Shared.LoggedInUser != null && !string.IsNullOrEmpty(Shared.LoggedInUser.UserName))
                    {
                        return SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần: Console.WriteLine(ex.Message);
                }
                return "Unknown"; // Trả về giá trị mặc định thay vì làm sập App
            }
        }

        public static string UserCode
        {
            get
            {
                try
                {
                    if (Shared.UserPermission?.OnlineUserModel != null)
                    {
                        return Shared.UserPermission.OnlineUserModel.ma_tai_khoan;
                    }

                    if (Shared.LoggedInUser != null && !string.IsNullOrEmpty(Shared.LoggedInUser.UserName))
                    {
                        return SecurityController.Decrypt(Shared.LoggedInUser.UserName, "rynan_encrypt_remember");
                    }
                }
                catch { }
                return "N/A";
            }
        }
    }
}
