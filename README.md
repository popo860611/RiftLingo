# RiftLingo

RiftLingo 是 Windows 上的《英雄聯盟》聊天即時翻譯工具。它只擷取使用者框選的螢幕區域，透過 OCR 讀取聊天文字，再以 Google Cloud Translation 翻譯成繁體中文，最後顯示在獨立、置頂、可點穿的透明浮窗。

> 本專案與 Riot Games 無關，也未獲 Riot Games 認可。RiftLingo 不注入遊戲、不讀取遊戲記憶體、不攔截封包、不模擬鍵盤滑鼠。第三方工具仍不存在零風險保證，請自行評估並遵守 Riot 最新政策。

## 功能

- 框選聊天區，不分析整個畫面
- 英文、日文、韓文、越南文、泰文及印尼文 OCR
- Google Cloud Translation 自動辨識來源語言
- 台灣 LoL 玩家用語在地化
- 不過濾髒話或人身攻擊，不自行加重原意
- 透明置頂、滑鼠點穿、不搶鍵盤焦點
- 最多顯示最近三句翻譯
- `Ctrl+Alt+T` 顯示或隱藏浮窗
- `Ctrl+Alt+P` 暫停或繼續翻譯
- API 金鑰使用 Windows DPAPI 依目前帳號加密保存

## 使用需求

- Windows 10 或 Windows 11 x64
- 建議將遊戲設為「無邊框」模式
- Google Cloud Translation API 金鑰
- 從原始碼建置時需要 .NET 10 SDK

## 從原始碼執行

```powershell
git clone <你的 RiftLingo 倉庫網址>
cd RiftLingo
.\scripts\download-models.ps1
dotnet run --project .\src\RiftLingo\RiftLingo.csproj
```

第一次啟動後：

1. 將《英雄聯盟》切到無邊框模式並打開聊天框。
2. 在 RiftLingo 按「框選聊天區」。
3. 拖曳框住左下角聊天文字，不要包含小地圖。
4. 輸入 Google Cloud Translation API 金鑰。
5. 按「開始翻譯」。

## 建立可攜式版本

```powershell
.\scripts\build-portable.ps1
```

輸出位於 `outputs/RiftLingo-portable-win-x64.zip`，旁邊會產生 SHA-256 校驗檔。

## 隱私

- 截圖只存在記憶體中，不會寫入磁碟。
- 只有 OCR 後的純文字會傳送給 Google Cloud Translation。
- API 金鑰不會提交到 Git；本機設定位於 `%LOCALAPPDATA%\RiftLingo\settings.json`，金鑰內容由 Windows DPAPI 加密。

## 已知限制

- 第一版依賴畫面 OCR；字體縮放、聊天背景透明度及解析度會影響辨識率。
- 獨佔全螢幕可能遮住外部浮窗，請使用無邊框模式。
- 尚未在真實對局完成端到端驗證；單元測試與桌面視窗測試不等於實際遊戲驗證。
- 台灣口語詞庫目前是可擴充的基礎版本，不會改寫或審查髒話。

## 開發

```powershell
dotnet test .\RiftLingo.slnx
dotnet build .\RiftLingo.slnx -c Release
```

## 授權

MIT License。遊戲名稱與相關商標屬各自權利人所有。
