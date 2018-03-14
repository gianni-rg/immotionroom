namespace ImmotionAR.ImmotionRoom.TrackingService.ViewModels
{
    using System.Windows.Input;
    using Helpers;
    using MVVM;

    public sealed class AboutWindowViewModel : BaseViewModel
    {
        #region Private fields

        #endregion

        #region Properties

        private string m_ProductNameVersion;

        public string ProductNameVersion
        {
            get { return m_ProductNameVersion; }
            set { Set(ref m_ProductNameVersion, value); }
        }

        private string m_CopyrightNotice;

        public string CopyrightNotice
        {
            get { return m_CopyrightNotice; }
            set { Set(ref m_CopyrightNotice, value); }
        }

        private string m_LicenseType;

        public string LicenseType
        {
            get { return m_LicenseType; }
            set { Set(ref m_LicenseType, value); }
        }

        private string m_LicenseOwner;

        public string LicenseOwner
        {
            get { return m_LicenseOwner; }
            set { Set(ref m_LicenseOwner, value); }
        }

        private string m_LicenseExpiredMessage;

        public string LicenseExpiredMessage
        {
            get { return m_LicenseExpiredMessage; }
            set { Set(ref m_LicenseExpiredMessage, value); }
        }

        #endregion

        #region Constructor

        public AboutWindowViewModel()
        {
            ProductNameVersion = string.Format("Immotionar ImmotionRoom TrackingService v{0}", AppVersions.RetrieveExecutableVersion());
            CopyrightNotice = "Copyright © 2017-2018 Gianni Rosa Gallina.\nCopyright © 2014-2017 Immotionar.\n\n";
            CopyrightNotice += "This program is free software: you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published by\nthe Free Software Foundation, either version 3 of the License, or\n(at your option) any later version.\n\n";
            CopyrightNotice += "This program is distributed in the hope that it will be useful,\nbut WITHOUT ANY WARRANTY; without even the implied warranty of\nMERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the\nGNU General Public License for more details.\n\n";
            CopyrightNotice += "You should have received a copy of the GNU General Public License\nalong with this program. If not, see <https://www.gnu.org/licenses/>.";
        }

        #endregion

        #region Commands

        public ICommand ExitCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () => { }
                };
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
        }

        #endregion
    }
}
