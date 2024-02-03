using CrucioNetwork;
using CrucioNetwork.Model;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrucioBackupper.ViewModel
{
    public class LoginViewModel(CrucioApi api): INotifyPropertyChanged
    {
        private readonly CrucioApi api = api;
        private readonly ILogger logger = Log.ForContext<LoginViewModel>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Nickname { get; private set; }
        public bool IsLoggedIn { get; private set; } = false;
        public bool IsVip { get; private set; } = false;
        public bool IsSvip { get; private set; } = false;
        public string? AvatarUuid { get; private set; }
        public string? AvatarUrl => AvatarUuid == null ? null : CrucioApi.GetImageUrl(AvatarUuid);

        public async Task LoginViaToken(string token)
        {
            api.SetToken(token);
            if (string.IsNullOrEmpty(token))
            {
                if (IsLoggedIn)
                {
                    IsLoggedIn = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
                }
                return;
            }
            var user = await api.GetCurrentUserInfo();
            if (!user.HasError && (user.Data?.Users?.Count ?? 0) > 0)
            {
                if (!IsLoggedIn)
                {
                    IsLoggedIn = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
                }
                var userInfo = user!.Data!.Users[0];
                if (Nickname != userInfo.Name)
                {
                    Nickname = userInfo.Name;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nickname)));
                }
                if (IsVip != userInfo.IsVip)
                {
                    IsVip = userInfo.IsVip;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVip)));
                }
                if (IsSvip != userInfo.IsSvip)
                {
                    IsSvip = userInfo.IsSvip;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSvip)));
                }
                if (AvatarUuid != userInfo.AvatarUuid)
                {
                    AvatarUuid = userInfo.AvatarUuid;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvatarUuid)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvatarUrl)));
                }
            }
            else
            {
                logger.Error("登录失败：(Code: {Code}) {Msg}", user.Code, user.Msg);
                if (IsLoggedIn)
                {
                    IsLoggedIn = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
                }
                api.SetToken(null);
            }
        }

        public void Logout()
        {
            api.SetToken(null);
            if (IsLoggedIn)
            {
                IsLoggedIn = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
        }
    }
}
