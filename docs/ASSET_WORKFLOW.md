# 素材の共同制作

- `Assets/_Project/Content/Features/<FeatureName>/<AssetName>/` を制作単位にします。コード側と同じ機能名を使ってください。
- 一つの素材の Prefabs、Models、Textures、Materials、Animations を同じ素材フォルダ内にまとめます。必要なディレクトリだけ作成します。
- 複数機能に使う素材は Content/Common に移します。購入品・外部パッケージは Assets/ThirdParty または Packages の所定の場所に置き、独自変更は _Project に置きます。
- ファイル名は `<Feature>_<Asset>_<Variant>` を基本とし、空白・連番の「final2」等を避けます。例: Player_Robot_BaseColor.png。
- 役割分担は素材または Prefab 単位で行います。同じ Scene/Prefab/バイナリ素材を同時に編集しないでください。機能ごとの Scene と Prefab を組み合わせ、Bootstrap シーンの編集担当を絞ります。
- Assets 内のファイル・ディレクトリの移動や改名は Unity Editor で行い、必ず .meta と一緒にコミットします。.meta の再生成や GUID の手編集は参照切れの原因になります。
- Force Text と Visible Meta Files を有効化済みです。Scene/Prefab の競合は UnityYAMLMerge または担当者による統合後、Unity で参照を確認します。競合マーカーを残したまま開かないでください。
- 画像・音声・動画・モデル・元データは .gitattributes の Git LFS 対象です。各参加者は Git LFS をインストールし、clone 前に `git lfs install` を実行してください。既存 clone は `git lfs pull` で実体を取得します。
- ArtSource に元データ、Content に Unity 向けの書き出しを置きます。元データと書き出しの更新を同じ PR に含め、対象機能・用途・サイズ・利用条件を記載します。
- 外部素材には同じフォルダに SOURCE.md を添え、入手先・作者・ライセンス・改変内容を書きます。公開リポジトリに再配布できる素材だけ追加します。
- 新しい拡張子の大容量バイナリを追加する際は、先に `git lfs track "*.拡張子"` を実行し .gitattributes を共有します。Unity の YAML (.unity/.prefab/.asset/.meta) は通常の Git 管理です。
