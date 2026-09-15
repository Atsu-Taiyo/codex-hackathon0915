# Features
機能追加時に `<FeatureName>/Model`, `Repo`, `Service`, `Controller`, `View/Components` のうち必要なディレクトリだけ作成します。
名前空間は `Hackathon.Features.<FeatureName>.<Role>`。View は Controller を通して操作し、データアクセスは Repo、業務処理は Service に分離します。
他機能の内部実装に直接依存せず、共有契約は必要になった時点で Common に抽出します。
