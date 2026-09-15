# codex-hackathon0915

Unity **6000.4.9f1** の共同開発用プロジェクト。Unity 標準の Assets / Packages / ProjectSettings をリポジトリ直下に置きます。

## 開始

1. Unity Hub で 6000.4.9f1 と Git LFS をインストール。
2. `git lfs install` の後に clone し、Unity Hub の Add project from disk でリポジトリ直下を選択。
3. `Assets/_Project/Scenes/WorldMap.unity` を開いて Play。3DのMapから **Enter Room 1** で言語探索パズルに入ります。

## World Map

PR #2の地形キットを組み合わせた島をcodex roboで歩けます。WASD／矢印キー、床クリックに対応。工房の近くでE／Enterを押すか、**Enter Room 1** をクリックするとRoom 1へ移動します。Room 1のMapボタン、クリア画面の **Back to map** で戻れます。単語・収集・クリア状況は往復しても保存されます。

Room 2は準備中の表示です。起動シーン・生成元・検証手順は [Mapガイド](docs/WORLD_MAP.md) を参照してください。Room 1だけを確認する場合は従来の `Bootstrap.unity` を直接開けます。

## Room 1 MVP

ロボットと箱をクリックして名詞を発見し、6種類の動詞と組み合わせて実験します。カードのドラッグ並べ替え、ウィジェット内の RUN、生成画像による3コマ再生、目標判定、図鑑・実行履歴・ローカル保存に対応しています。

操作と検証手順は [Room 1 ガイド](docs/ROOM_ONE.md)、画像生成プロンプトは [制作記録](ArtSource/RoomOne/PROMPTS.md) を参照してください。

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

機能テストは `Assets/_Project/Tests/EditMode/Features/<FeatureName>/` または `Tests/PlayMode/Features/<FeatureName>/` に置き、実装名と対応する `*Tests.cs` を使用します。Core/Common も同じ対応で整理します。Unity Test Framework を使う際は Runtime/Test の assembly definition を導入し、Editor 専用テストは Editor に限定します。Room 1 には追加パッケージ不要の Editor 検証コマンドと Play モード検証があり、手順は Room 1 ガイドに記載しています。

## 共同作業

機能・素材ごとのブランチと PR を使います。Library/Temp/Logs/UserSettings は共有せず、Assets の .meta、Packages の manifest/lock、ProjectSettings は共有します。
素材配置、元データ、競合回避、命名、LFS は [素材制作ガイド](docs/ASSET_WORKFLOW.md) を参照してください。
