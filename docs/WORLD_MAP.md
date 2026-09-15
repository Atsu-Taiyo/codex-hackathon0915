# World Map

## 起動と操作

`Assets/_Project/Scenes/WorldMap.unity` をUnity 6000.4.9f1で開いてPlay。

- **WASD / 矢印キー**でcodex roboを移動。**床クリック**で目的地まで直進します。
- **01 Workshop** のラベルをクリックすると入口へ歩きます。
- 工房の入口付近で **E / Enter**、または右下の **Enter** でRoom 1へ移動します。
- Room 1の左上ホーム、右下Map、クリア画面の **Back to map** でMapへ戻ります。アニメーション実行中・音声処理中は遷移を待ちます。
- クリア済みの工房にはチェックを表示します。詳しい単語・収集数はRoom 1で確認できます。保存形式は既存の `Hackathon.RoomOne.Progress.v1` を使い、入室時に進捗を消去しません。
- Room 2は準備中。ラベルの選択と入口までの移動はできますが、入室機能はありません。

MapからRoom 1への移動ではシーンを切り替え、戻るとロボットはMapの開始地点に立ちます。カードの未実行の並びは部屋を出るとリセットされます。クリック移動の自動迂回はありません。

## 取得したPR

[PR #2: Add modular terrain kit for Room 1, Room 2 and future backdrops](https://github.com/Atsu-Taiyo/codex-hackathon0915/pull/2)

取得リビジョン: `8fa588c79f6ecd0121bc3d65acdbb388a099e209`。

PRの追加ファイル55点を取得し、18個のFBX実データ、Blender制作元、パレット、検証記録、プレビューを取り込みました。既存のRoom 1や音声機能をPRの古い実装へ戻さないよう、追加された地形関連ファイルだけを採用しています。

草地、砂地、崖、工房の床・壁、台地、3種類の遠景をMapに配置。通常の地形にはPRのセットアップが生成するMeshColliderを使い、島の周囲には落下防止の当たり判定を追加しています。

工房には窓・作業台・工具・ひさし・ランタン、海岸には桟橋・岩・岸辺の斜面を配置しました。草地には坂・階段でつながる高台と、低ポリゴンの樹木・低木・花を追加。共有メッシュとマテリアルは `Map/IslandArt` に保存し、水面は軽量な時間変化のあるシェーダーで描画します。装飾の多くは衝突判定を持たず、入口への道を歩けます。

## 構成

- `WorldMap.unity`: 保存済みのMapシーン。ビルドの先頭。
- `Bootstrap.unity`: 既存のRoom 1。ビルドの2番目。
- `WorldNavigation`: 両シーン間の移動。
- `WorldMapController`: 操作案内、部屋選択、近接入室、保存状況表示。
- `RoomOneNavigation`: 処理中の遷移を防ぎ、Mapへ戻る。
- `TerrainKit/Prefabs`, `TerrainKit/Materials`: 18個の地形Prefabと共有マテリアル。
- `Map/CodexRobo/CodexRobo.prefab`: 既存の歩行ロボットを参照。

## 再生成とビルド

**Tools > World Map > Create Map and Connect Room 1** はMapが存在しない場合のみ生成します。既存シーンの編集を保護するため、存在するMapを上書きしません。地形Prefabとマテリアルも既存のものを再利用します。

**Tools > World Map > Refresh Island Artwork** は既存Mapのアートを更新して保存します。実行前にシーンを `Temp/ArtBackups` へ別名で退避します。`World Map Art` 内は生成領域なので再実行で置き換わります。旧ツリー・小道などは非表示で保持します。再生成領域の手編集を残したい場合は、このコマンドを実行せずシーンを直接編集してください。

**Tools > World Map > Build macOS** で `Builds/EnglishWorld.app` を作成します。Webビルドと既存のRoom Oneビルドコマンドも有効なBuild Settingsの全シーンを含みます。

## 検証手順

MapのPlay中に **Tools > World Map > Run Navigation Playtest** を実行します。地形マテリアル、床上の歩行、工房への到達、外周衝突、Map → Room 1 → Map → Room 1の遷移、クリアと収集・履歴の保存を確認します。開始前のPlayerPrefsを退避し、終了時・中断時に復元します。結果は `Temp/WorldMap-playtest.txt`。

検証用プロジェクトのコピーでは `-batchmode -executeMethod Hackathon.Editor.WorldMapBatchVerify.Run` を使用できます。結果はコピー内の `Verification/WorldMap-playtest.txt` に残ります。
