using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using HospitalManagement.Helpers;
using HospitalManagement.Models;
using HospitalManagement.Services;
using Microsoft.Win32;

namespace HospitalManagement.ViewModels.Admin
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly AdminService _adminService = new();

        private User? _selectedUser;
        private string _newUsername = string.Empty;
        private string _newPassword = string.Empty;
        private string _newFullname = string.Empty;
        private string _newRole = "Doctor";
        private string _resetPasswordText = string.Empty;
        private string _backupPath = @"C:\Backups\G5_QLBV.bak";
        private string _statusMessage = string.Empty;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Syslog> SysLogs { get; } = new();
        public string[] AvailableRoles { get; } = { "Admin", "Management", "Doctor", "Nurse", "Reception", "Pharmacy" };

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string NewUsername { get => _newUsername; set => SetProperty(ref _newUsername, value); }
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        public string NewFullname { get => _newFullname; set => SetProperty(ref _newFullname, value); }
        public string NewRole { get => _newRole; set => SetProperty(ref _newRole, value); }
        public string ResetPasswordText { get => _resetPasswordText; set => SetProperty(ref _resetPasswordText, value); }
        public string BackupPath { get => _backupPath; set => SetProperty(ref _backupPath, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand CreateAccountCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand RefreshAllCommand { get; }
        public ICommand RefreshLogsCommand { get; }
        public ICommand BrowseBackupCommand { get; }
        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }

        public AdminViewModel()
        {
            CreateAccountCommand = new RelayCommand(_ => CreateAccount());
            ToggleLockCommand = new RelayCommand(_ => ToggleLock());
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword());
            RefreshAllCommand = new RelayCommand(_ => RefreshAll());
            RefreshLogsCommand = new RelayCommand(_ => LoadLogs());
            BrowseBackupCommand = new RelayCommand(_ => BrowseBackupFile());
            BackupCommand = new RelayCommand(_ => ExecuteBackup());
            RestoreCommand = new RelayCommand(_ => ExecuteRestore());

            RefreshAll();
        }

        public void RefreshAll()
        {
            LoadUsers();
            LoadLogs();
        }

        private void LoadUsers()
        {
            Users.Clear();
            foreach (var u in _adminService.GetAllUsers()) Users.Add(u);
        }

        private void LoadLogs()
        {
            SysLogs.Clear();
            foreach (var l in _adminService.GetSysLogs()) SysLogs.Add(l);
        }

        private void CreateAccount()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(NewFullname))
            {
                StatusMessage = "Vui lòng nhập đầy đủ thông tin tài khoản.";
                return;
            }
            if (_adminService.CreateUser(NewUsername.Trim(), NewPassword, NewFullname.Trim(), NewRole, out string msg))
            {
                NewUsername = string.Empty;
                NewPassword = string.Empty;
                NewFullname = string.Empty;
                RefreshAll();
            }
            StatusMessage = msg;
        }

        private void ToggleLock()
        {
            if (SelectedUser == null)
            {
                RefreshAll();
                return;
            }
            bool ok = _adminService.ToggleUserLock(SelectedUser.UserId);
            StatusMessage = ok
                ? $"Đã thay đổi trạng thái tài khoản {SelectedUser.Username}."
                : "Tài khoản không còn tồn tại trong CSDL. Đã làm mới danh sách.";
            RefreshAll();
        }

        private void ResetPassword()
        {
            if (SelectedUser == null || string.IsNullOrWhiteSpace(ResetPasswordText))
            {
                StatusMessage = "Chọn tài khoản và nhập mật khẩu mới.";
                return;
            }
            bool ok = _adminService.ResetPassword(SelectedUser.UserId, ResetPasswordText);
            ResetPasswordText = string.Empty;
            StatusMessage = ok
                ? $"Đã cấp lại mật khẩu cho {SelectedUser.Username}."
                : "Tài khoản không còn tồn tại trong CSDL. Đã làm mới danh sách.";
            RefreshAll();
        }

        private void BrowseBackupFile()
        {
            var dlg = new SaveFileDialog { Filter = "SQL Backup (*.bak)|*.bak", FileName = "G5_QLBV.bak" };
            if (dlg.ShowDialog() == true) BackupPath = dlg.FileName;
        }

        private void ExecuteBackup()
        {
            try
            {
                _adminService.BackupDatabase(BackupPath);
                StatusMessage = "Sao lưu cơ sở dữ liệu thành công.";
                LoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRestore()
        {
            try
            {
                _adminService.RestoreDatabase(BackupPath);
                StatusMessage = "Phục hồi cơ sở dữ liệu thành công.";
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi phục hồi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}