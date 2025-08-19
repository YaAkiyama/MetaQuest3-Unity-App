# VRアプリ開発 継続作業指示書

## 現在の状態（2025年8月19日 11:50時点）

### 環境設定 - 完了済み
- **OS**: Windows 11
- **Unity**: 2022.3.62f1 LTS（VRテンプレート使用）
- **プロジェクト**: MQ3-Unity-App
- **プロジェクトパス**: `D:\project\Unity\MQ3-Unity-App`
- **GitHubリポジトリ**: https://github.com/YaAkiyama/MetaQuest3-Unity-App
- **Node.js**: D:\nodejs\にインストール済み
- **Python**: 3.13（C:\Users\yasun\AppData\Roaming\Python\Python313）

### MCP設定 - 完了済み
- ✅ **Unity MCP**: justinpbarnett/unity-mcp（動作確認済み）
- ✅ **GitHub MCP**: @modelcontextprotocol/server-github（動作確認済み）
- ✅ **ClaudeCode**: インストール済み、MCP接続確認済み

### Unityパッケージ - インストール済み
- XR Interaction Toolkit
- XR Plugin Management (Oculus)
- Unity MCP Bridge (justinpbarnett/unity-mcp)

### 作成済みスクリプト
```
Assets/Scripts/VR/
├── VRManager.cs           # VRシステム全体の管理
├── HandInteractionController.cs  # ハンドトラッキング制御
└── GrabbableObject.cs     # インタラクティブオブジェクト
```

### Unity MCP利用可能ツール
1. `manage_script` - C#スクリプトの作成、読み込み、更新、削除
2. `manage_scene` - シーンのロード、保存、作成、階層取得
3. `manage_editor` - エディタの制御（再生、一時停止、ツール切り替え）
4. `manage_gameobject` - GameObjectの作成、変更、削除、コンポーネント操作
5. `manage_asset` - アセットのインポート、作成、変更、削除
6. `manage_shader` - シェーダースクリプトの管理
7. `read_console` - Unityコンソールメッセージの取得やクリア
8. `execute_menu_item` - Unityメニューコマンドの実行

## 次の作業：Meta Quest 3でのビルドとテスト

### 作業前の確認事項
1. **Unity Editorを起動**
   - Unity Hubから「MQ3-Unity-App」を開く
   - プロジェクトが完全にロードされるまで待つ

2. **ClaudeCodeを起動**（Unity MCPを使用する場合）
   ```cmd
   cd D:\project\Unity\MQ3-Unity-App
   npx @anthropic-ai/claude-code
   ```

### ビルド設定の手順

#### 1. Android Build Supportの確認
- Unity Hub → Installs → Unity 2022.3.62f1の歯車アイコン
- Add Modules → Android Build Support（インストール済みか確認）

#### 2. Build Settings
- File → Build Settings
- Platform: Android を選択
- Switch Platform ボタンをクリック（未実施の場合）

#### 3. Player Settings（重要）
**Edit → Project Settings → Player → Android Settings**

必須設定：
- **Company Name**: YaAkiyama（または任意）
- **Product Name**: MQ3UnityApp
- **Package Name**: com.yaakiyama.mq3unityapp
- **Minimum API Level**: Android 10.0 (API level 29)
- **Target API Level**: Automatic (highest installed)

#### 4. XR Plug-in Management設定
**Edit → Project Settings → XR Plug-in Management → Android**
- ✅ Oculus にチェック

#### 5. Oculus設定
**Edit → Project Settings → XR Plug-in Management → Oculus**
- **Stereo Rendering Mode**: Multiview
- **Target Devices**: ✅ Quest 3

### VRシーンのセットアップ（未実施）

以下の作業が必要です：

1. **基本VRシーンの作成**
   - Scenes/VRMainScene.unity を作成
   - XR Origin (VR) の配置
   - 床とテスト用オブジェクトの配置

2. **ハンドトラッキング設定**
   - OVR CameraRig の設定
   - Hand Tracking Support を有効化

3. **インタラクション設定**
   - XR Interaction Manager の追加
   - Grabbable オブジェクトの配置

### Meta Quest 3実機テスト準備

1. **開発者モードの有効化**
   - Meta Questアプリ（スマートフォン）→ 設定 → 開発者モード ON

2. **USBデバッグの許可**
   - Quest 3をUSBケーブルでPCに接続
   - ヘッドセット内で「USBデバッグを許可」を選択

3. **ビルドと実行**
   - File → Build and Run
   - APKファイル名を指定
   - 自動的にQuest 3にインストール・起動

## MCP設定ファイル（参考）

`.claude/mcp.json` の内容：
```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "C:\\Users\\yasun\\AppData\\Roaming\\Python\\Python313\\Scripts\\uv.exe",
      "args": [
        "--directory",
        "C:\\Users\\yasun\\AppData\\Local\\Programs\\UnityMCP\\UnityMcpServer\\src",
        "run",
        "server.py"
      ]
    },
    "github": {
      "command": "D:\\nodejs\\npx.cmd",
      "args": [
        "-y",
        "@modelcontextprotocol/server-github"
      ],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "[YOUR_TOKEN_HERE]"
      }
    }
  }
}
```

## トラブルシューティング

### Unity MCPが接続できない場合
1. Unity Editorが起動していることを確認
2. Unity MCP Bridgeパッケージがインストールされていることを確認
3. ClaudeCodeを再起動

### ビルドエラーが発生した場合
1. Android SDK/NDK/JDKが正しくインストールされているか確認
2. Unity Hub → Preferences → External Tools で設定を確認

## 作業継続用プロンプト

新しいClaudeDesktopチャットで以下をペーストしてください：

---

**【作業継続依頼】**

MetaQuest3向けVRアプリ開発の続きをお願いします。

GitHubリポジトリ: https://github.com/YaAkiyama/MetaQuest3-Unity-App
継続作業資料: HANDOVER_DOCUMENT.md を参照

現在の状態：
- Unity 2022.3.62f1 LTSでVRプロジェクト作成済み
- Unity MCPとGitHub MCP設定完了
- 基本的なVRスクリプト作成済み

次の作業：**Meta Quest 3でのビルドとテスト**

Unity MCPとGitHub MCPを使用して、以下を実行してください：
1. VRシーンのセットアップ（XR Origin配置など）
2. Android向けビルド設定の確認と調整
3. テスト用APKのビルド準備

プロジェクトパス: D:\project\Unity\MQ3-Unity-App
Unity Editorは起動済みです。

よろしくお願いします。
