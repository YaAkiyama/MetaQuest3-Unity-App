# MetaQuest3 Unity Application

## 概要
ClaudeCodeとUnityMCPを使用して開発するMetaQuest3向けVRアプリケーション

## 開発環境
- **OS**: Windows 11
- **Unity**: 2022.3.62f1 LTS
- **開発ツール**: 
  - ClaudeCode (Anthropic)
  - UnityMCP (Unity Editor Control)
  - GitHub MCP
- **ターゲットプラットフォーム**: Meta Quest 3

## セットアップ手順

### 1. リポジトリのクローン
```bash
git clone https://github.com/YaAkiyama/MetaQuest3-Unity-App.git
cd MetaQuest3-Unity-App
```

### 2. Unity プロジェクトの設定
1. Unity Hubでプロジェクトを開く（Unity 2022.3.62f1 LTS）
2. Build Settings → Android に切り替え
3. XR Plugin Management → Oculus を有効化

### 3. ClaudeCode の起動
```bash
npx @anthropic-ai/claude-code
```

## プロジェクト構造
```
MQ3-Unity-App/
├── Assets/
│   ├── Scripts/       # C# スクリプト
│   │   └── VR/       # VR関連スクリプト
│   │       ├── VRManager.cs
│   │       ├── HandInteractionController.cs
│   │       └── GrabbableObject.cs
│   ├── Scenes/        # シーン
│   │   ├── SampleScene.unity
│   │   └── VRMainScene.unity  # メインVRシーン
│   ├── Prefabs/       # プレハブ
│   ├── Materials/     # マテリアル
│   └── Textures/      # テクスチャ
├── Packages/          # パッケージ設定
├── ProjectSettings/   # プロジェクト設定
├── HANDOVER_DOCUMENT.md  # 作業継続用ドキュメント
└── README.md
```

## 実装済み機能
- ✅ 基本的なVRシーン（VRMainScene）
- ✅ XR Originセットアップ
- ✅ インタラクティブオブジェクト配置
- ✅ VR管理スクリプト群

## 開発中の機能
- 🔧 ハンドトラッキング対応
- 🔧 空間認識機能
- 🔧 高度なインタラクション

## ビルド方法

### 事前準備
1. **Quest 3の開発者モード有効化**
   - Meta Questアプリ → 設定 → 開発者モード ON
2. **USB接続とデバッグ許可**
   - Quest 3をUSB-Cケーブルで接続
   - ヘッドセット内で「USBデバッグを許可」

### ビルド手順
1. File → Build Settings
2. Platform: Android を選択 → Switch Platform
3. Add Open Scenes でVRMainSceneを追加
4. Player Settings:
   - Package Name: `com.yaakiyama.mq3unityapp`
   - Minimum API Level: `Android 10.0 (API level 29)`
5. XR Plugin Management → Oculus にチェック
6. Build And Run

## トラブルシューティング
- **Unity MCPエラー**: Unity Editorを再起動
- **ビルドエラー**: Android SDK/NDK/JDKの設定を確認
- **Quest 3認識エラー**: USBデバッグの再許可

## ライセンス
MIT License

## 作成者
YaAkiyama

## 更新履歴
- 2025-08-19: VRMainSceneセットアップ、ビルド設定手順追加
- 2025-08-18: プロジェクト作成、基本スクリプト実装