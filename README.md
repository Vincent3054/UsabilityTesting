# Usability Testing / 網站可用性監控服務

這是一個基於 .NET 8 Worker Service 的輕量級網站可用性監控工具。它設計用於部署在 Kubernetes (AKS) 環境中，能夠定期檢查指定的 HTTP 端點，並在服務狀態發生變化（正常 ↔ 異常）時發送 Email 通知。

## ✨ 功能特色

- **Excel/CSV 設定驅動**: 透過 Excel 檔案輕鬆管理監控目標。
- **HTTP 狀態監控**: 支援自訂 HTTP Method、Headers 與 Body。
- **智慧重試機制 (Smart Retry)**: 整合 Polly，在判定失敗前自動重試 (預設 3 次)，防止網路抖動造成的誤報。
- **狀態追蹤 (Stateful Alerting)**: 僅在服務狀態改變時發送通知 (例如：從「正常」變為「異常」，或從「異常」恢復為「正常」)，避免信件轟炸。
- **彈性架構**: 採用 `ITargetProvider` 介面設計，未來可輕鬆擴充支援資料庫或其他設定來源。
- **容器化支援**: 內建 Dockerfile 與 Kubernetes 部署設定。

## 🛠️ 架構設計

- **Worker Service**: 核心後景服務，依據 `CheckIntervalSeconds` 定期執行。
- **ExcelTargetProvider**: 負責讀取監控目標設定 (預設路徑: `config/targets.xlsx`)。
- **HttpMonitor**: 執行實際的 HTTP 請求檢查。
- **StatusTracker**: 維護每個服務的健康狀態，決定是否觸發警報。
- **EmailNotifier**: 透過 SMTP 發送警報郵件。

## 🚀 快速開始

### 前置需求
- .NET 8 SDK
- Docker (選用，用於容器化部署)

### 本地執行
1. 複製專案到本地。
2. 在 `UsabilityTesting.Worker` 目錄下建立 `config` 資料夾，並放入 `targets.xlsx` (或修改 `appsettings.json` 指向你的檔案)。
3. 修改 `appsettings.json` 設定 SMTP 資訊。
4. 執行專案：
   ```bash
   dotnet run --project UsabilityTesting.Worker
   ```

## ⚙️ 設定說明 (Configuration)

主要設定位於 `appsettings.json`：

```json
{
  "MonitorSettings": {
    "ExcelFilePath": "config/targets.xlsx", // Excel 設定檔路徑
    "CheckIntervalSeconds": 300,            // 檢查間隔 (秒)
    "RetryCount": 3,                        // 失敗重試次數
    "RetryDelayMilliseconds": 2000          // 重試間隔 (毫秒)
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "your_username",
    "Password": "your_password",
    "FromEmail": "monitor@example.com",
    "DisplayName": "Usability Monitor"
  }
}
```

### Excel 欄位格式
請確保 Excel 檔案包含以下欄位名稱：

| 欄位名稱 | 說明 | 範例 |
|----------|------|------|
| `Name` | 服務名稱 | Google Homepage |
| `Url` | 監控網址 | https://www.google.com |
| `Method` | HTTP 方法 | GET |
| `Headers` | 請求標頭 (選填) | Key:Value;Auth:Token |
| `Body` | 請求內容 (選填) | {"id": 1} |
| `ExpectedStatusCode` | 預期狀態碼 | 200 |
| `NotifyEmails` | 通知信箱 (分號分隔) | admin@example.com;dev@example.com |

## 📦 部署 (Deployment)

### Docker Build
```bash
docker build -t usability-monitor -f UsabilityTesting.Worker/Dockerfile .
```

### Kubernetes (AKS) Deploy
專案內附 `k8s-deployment.yaml` 範本。

1. **ConfigMap**: 可將 `appsettings.json` 或 `targets.xlsx` 透過 ConfigMap 掛載。
2. **Deployment**:
   ```bash
   kubectl apply -f k8s-deployment.yaml
   ```

## 📂 專案結構

```
UsabilityTesting/
├── UsabilityTesting.Worker/      # Worker Service 主專案
│   ├── Interfaces/               # 介面定義 (ITargetProvider)
│   ├── Models/                   # 資料模型
│   ├── Services/                 # 核心邏輯 (HttpMonitor, EmailNotifier...)
│   ├── Worker.cs                 # 背景任務進入點
│   ├── Dockerfile                # 容器建置檔
│   └── appsettings.json          # 設定檔
└── k8s-deployment.yaml           # K8s 部署範本
```
