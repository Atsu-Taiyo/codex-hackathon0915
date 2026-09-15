# CI / CD

- GitHub の main push / PR で `CI` が Unity プロジェクト設定を検査します。これはコンパイルテストではありません。
- Draft 以外の PR 作成・再開・更新時に `@codex review` を同じコミットにつき一度コメントします。Codex 側の対象リポジトリの Code review 有効化が必要です。
- この Codex タスクの定期実行が main の成功した CI を確認し、専用の clean checkout で Unity 6000.4.9f1 の WebGL ビルドを実行して Sites に公開します。
- Mac と Codex が実行できる状態である必要があります。GitHub Actions 用の Unity ライセンスや Sites の長期トークンは保存しません。
- 元の作業ディレクトリの未コミット変更は公開対象にしません。main に push されたソースだけを公開します。PR のマージは自動化しません。
- Sites の作成・認証・保存・公開は Sites スキルと専用ツールを使います。公開成功を確認した main SHA だけを完了として記録します。ビルドや公開の失敗時は既存の公開版を維持します。

## Web ビルド

専用の checkout 内で実行します（Unity Editor で開いているプロジェクトでは実行しないでください）。

```sh
/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath "$PWD" -buildTarget WebGL -executeMethod Hackathon.Editor.WebBuild.Build -logFile web-build.log
```

出力は `Builds/WebGL/index.html`。Gzip の展開フォールバックを有効にし、通常の静的ホスティングで配信します。
