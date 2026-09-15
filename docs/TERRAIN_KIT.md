# 地形キット

Room1の工房に合わせた床・壁・壁の角の3点、Room2の候補となる屋外地形12点、Room3以降を想定した遠景3点を、個別のFBXとして追加しました。仕掛けパーツや部屋全体のマップは含んでいません。Room2の屋外設定は提案であり、遠景3点は背景用です。

## Unityでの使い方

1. `git lfs pull`でモデルと制作元データを取得します。
2. Unityでプロジェクトを開き、`Tools > Terrain Kit > Create Individual Prefabs`を実行します。
3. `Assets/_Project/Content/Common/TerrainKit/Prefabs`から必要なパーツを配置します。

補助スクリプトは、色付きマテリアルと個別のPrefabを生成します。通常の地形には静的なMeshColliderを付与し、`Backdrop_`で始まる遠景には当たり判定を付けません。Built-inとURP向けとなっています。なお、再実行時に既存のPrefabやマテリアルを上書きすることはありません。

Unity Editorでのインポート、描画、当たり判定、C#のコンパイルは未検証です。現在のRoom1は2D画像とIMGUIで描画されており、追加した素材への切り替えはこの変更には含んでいません。

## プレビュー

画像は今回の3DモデルをBlenderで描画したものです。

### Room1：工房

![木の床・壁・壁の角](TerrainKit/Room1_Workshop_Parts.png)

### Room2：屋外地形の候補

![草地・砂地・崖・坂・階段など](TerrainKit/Terrain_Parts.png)

### Room3以降：背景用の地形

![砂岩の台地・氷の段丘・岩のアーチ](TerrainKit/Later_Stage_Backdrops.png)

## 寸法と検証

基本は2mグリッドで、Unityに合わせてY軸を上としています。平らな床の歩行面、壁や遠景の下端をY=0にしています。坂は低い側、岸辺の斜面は高い側がY=0です。坂の高低差は公称1m、階段は0.25mずつ4段となっています。角には小さな面取りを入れています。

18モデル、合計10,924三角形です。全モデルで開いた辺や面積ゼロの面を検査し、FBXをBlenderへ再読み込みして寸法、三角形数、原点、UV、マテリアル名を照合しました。詳細は[メッシュ検証](../ArtSource/Common/TerrainKit/Validation/mesh-report.json)と[FBX再読み込み検証](../ArtSource/Common/TerrainKit/Validation/fbx-roundtrip.json)をご確認ください。

## 制作元

- [編集用Blenderファイル](../ArtSource/Common/TerrainKit/TerrainKit.blend)
- [再生成の手順](../ArtSource/Common/TerrainKit/README.md)
- [出典と用途](../Assets/_Project/Content/Common/TerrainKit/SOURCE.md)

すべて新規に作成したメッシュと単色マテリアルで構成されており、外部テクスチャは不要です。FBX、Blenderファイル、プレビュー画像はGit LFSで管理します。
