# EmployeeManager

一個使用 ASP.NET Core MVC 建立的員工管理系統，具備以下功能：
- 員工新增、編輯、刪除 (CRUD)
- 員工清單搜尋、排序與分頁
- 刪除確認 Bootstrap Modal
- CSV 匯出員工清單
- 資料庫存取 (Entity Framework Core)
- 日誌記錄 (Logger)

## 技術
- ASP.NET Core MVC
- Entity Framework Core
- Bootstrap 5
- NLog

## 安裝
1. Clone 專案：
```bash
git clone https://github.com/k8731/EmployeeManager

2. 開啟專案，確保已安裝必要 NuGet 套件：
dotnet restore

3. 執行專案：
dotnet run