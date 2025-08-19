# MetaQuest3 Unity Application

## 概要
ClaudeCodeとUnityMCPを使用して開発するMetaQuest3向けVRアプリケーション

## 開発環境
- **OS**: Windows 11
- **Unity**: 2022.3 LTS以降
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
1. Unity Hubでプロジェクトを開く
2. Build Settings → Android に切り替え
3. XR Plugin Management → Oculus を有効化

### 3. ClaudeCode の起動
```bash
claude-code
```

## プロジェクト構造
```
MetaQuest3-Unity-App/
├── Assets/
│   ├── Scripts/       # C# スクリプト
│   ├── Prefabs/       # プレハブ
│   ├── Materials/     # マテリアル
│   ├── Textures/      # テクスチャ
│   └── Scenes/        # シーン
├── Packages/          # パッケージ設定
├── ProjectSettings/   # プロジェクト設定
└── README.md
```

## 機能
- VR インタラクション
- ハンドトラッキング対応
- 空間認識機能

## ビルド方法
1. File → Build Settings
2. Platform: Android を選択
3. Texture Compression: ASTC
4. Build And Run

## ライセンス
MIT License

## 作成者
YaAkiyama

## 更新履歴
- 2025-08-18: プロジェクト作成