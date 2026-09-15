# codex-hackathon0915

Unity **6000.4.9f1** の共同開発用プロジェクト。Unity 標準の Assets / Packages / ProjectSettings をリポジトリ直下に置きます。

## 開始

1. Unity Hub で 6000.4.9f1 と Git LFS をインストール。
2. `git lfs install` の後に clone し、Unity Hub の Add project from disk でリポジトリ直下を選択。
3. `Assets/_Project/Scenes/Bootstrap.unity` を開いて Play。Console に `[Hackathon] Application initialized.` を出力します。

## 配置

```text
Assets/_Project/
├── Scripts/
│   ├── Core/                 # アプリ初期化・機能の組み立て
│   ├── Common/               # 複数機能の共有コード
│   └── Features/
│       └── <FeatureName>/    # 機能追加時に必要な役割だけ作成
│           ├── Model/
│           ├── Repo/
│           ├── Service/
│           ├── Controller/
│           └── View/
│               └── Components/
├── Content/
│   ├── Common/
│   └── Features/<FeatureName>/<AssetName>/
├── Scenes/                   # アプリ起動シーン
└── Editor/                   # Unity Editor 専用の基盤ツール
ArtSource/                    # Unity に取り込まない制作元データ
```

C# の慣習に合わせディレクトリ名は PascalCase。機能実装は Features に集め、Core はアプリ全体の初期化、Common は実際に共有するものに限定します。Repo は取得・保存、Service は業務処理・外部連携、Controller は状態・操作、View は表示を担当します。ゲーム機能が未定のため、ダミー機能や不要な役割フォルダは作成していません。

## テストの追加

機能テストは `Assets/_Project/Tests/EditMode/Features/<FeatureName>/` または `Tests/PlayMode/Features/<FeatureName>/` に置き、実装名と対応する `*Tests.cs` を使用します。Core/Common も同じ対応で整理します。初めてテストを追加する際に Unity Test Framework と Runtime/Test の assembly definition を導入し、Editor 専用テストは Editor に限定します。現時点ではゲーム機能・テストスイートはありません。

## 共同作業

機能・素材ごとのブランチと PR を使います。Library/Temp/Logs/UserSettings は共有せず、Assets の .meta、Packages の manifest/lock、ProjectSettings は共有します。
素材配置、元データ、競合回避、命名、LFS は [素材制作ガイド](docs/ASSET_WORKFLOW.md) を参照してください。
