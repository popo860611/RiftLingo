# RiftLingo

RiftLingo 是 Windows 上的《英雄聯盟》聊天即時翻譯工具。它只擷取使用者框選的聊天區，交由 Gemini 同時辨識與翻成台灣繁體中文，最後顯示在獨立、置頂、可點穿的透明浮窗。

> 本專案與 Riot Games 無關，也未獲 Riot Games 認可。RiftLingo 不注入遊戲、不讀取遊戲記憶體、不攔截封包、不模擬鍵盤滑鼠。第三方工具仍不存在零風險保證，請自行評估並遵守 Riot 最新政策。

## 功能

- 框選聊天區，不分析整個畫面
- 一鍵測試 Gemini 圖片翻譯，可先確認框選與翻譯結果
- Gemini 直接理解聊天截圖，不需要安裝 Tesseract 或語言模型
- 畫面沒有明顯變化時不呼叫 API，節省免費額度
- 訊息去重，避免同一句翻譯反覆出現
- 自動辨識英文、日文、韓文、越南文、泰文、印尼文等語言
- 台灣 LoL 玩家用語在地化
- 不過濾髒話或人身攻擊，不自行加重原意
- 透明置頂、滑鼠點穿、不搶鍵盤焦點
- `Ctrl+Alt+T` 顯示或隱藏浮窗
- `Ctrl+Alt+P` 暫停或繼續翻譯
- API Key 使用 Windows DPAPI 依目前帳號加密保存

## 使用需求

- Windows 10 或 Windows 11 x64
- 建議將遊戲設為「無邊框」模式
- Google AI Studio 建立的 Gemini API Key；可使用 Gemini API 免費層
- 從原始碼建置時需要 .NET 10 SDK

## 從原始碼執行

```powershell
git clone https://github.com/popo860611/RiftLingo.git
cd RiftLingo
dotnet run --project .\src\RiftLingo\RiftLingo.csproj
```

第一次啟動後：

1. 將《英雄聯盟》切到無邊框模式並打開聊天框。
2. 在 RiftLingo 按「框選聊天區」。
3. 拖曳框住左下角聊天文字，不要包含小地圖或其他私人資訊。
4. 輸入在 Google AI Studio 建立的 Gemini API Key。
5. 先按「測試 Gemini 翻譯」，確認截圖與結果。
6. 按「開始翻譯」。

## 建立可攜式版本

```powershell
.\scripts\build-portable.ps1
```

輸出位於 `outputs/RiftLingo-portable-win-x64.zip`，旁邊會產生 SHA-256 校驗檔。

## 隱私

- 截圖只存在記憶體中，不會寫入磁碟。
- 使用者框選的聊天區截圖會傳送給 Google Gemini API。
- Gemini 免費層提交內容可能用於改善 Google 產品；付費層適用不同資料條款，請以 Google 官方說明為準。
- API Key 不會提交到 Git；本機設定位於 `%LOCALAPPDATA%\RiftLingo\settings.json`，金鑰內容由 Windows DPAPI 加密。

## 已知限制

- 圖片辨識品質仍會受到聊天字體大小、透明度與解析度影響。
- Gemini 免費層有速率與每日額度限制；額度用完時需等待重置或改用付費層。
- 獨佔全螢幕可能遮住外部浮窗，請使用無邊框模式。
- 尚未在真實對局完成端到端驗證；單元測試與桌面視窗測試不等於實際遊戲驗證。

## 開發

```powershell
dotnet test .\RiftLingo.slnx
dotnet build .\RiftLingo.slnx -c Release
```

## 授權

MIT License。遊戲名稱與相關商標屬各自權利人所有。
