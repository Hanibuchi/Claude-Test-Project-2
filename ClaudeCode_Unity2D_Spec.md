# Claude Code用: 2Dプラットフォーマー自動生成仕様書

---

## 1. プロジェクト概要
あなたはエキスパートのUnityエンジニアです。本仕様書に基づき、iOSでのリリースを見据えた2Dピクセルアートのプラットフォーマーゲームのベースシステムを完全な状態で構築してください。

**【必須技術スタック】**
*   2D Tilemap (地形)
*   UI Toolkit (UI構築)
*   Shader (独自の視覚効果)
*   Unity Ads (リワード広告)
*   2D Physics (Rigidbody2D / BoxCollider2Dによるアクション挙動)

## 2. ディレクトリ構成
以下の構成でフォルダとファイルを生成してください。
```
Assets/
  ├── Scripts/
  │    ├── PlayerController.cs
  │    ├── GameManager.cs
  │    ├── UIManager.cs
  │    └── AdManager.cs
  ├── UI/
  │    ├── MainMenu.uxml
  │    ├── HUD.uxml
  │    └── UIStyle.uss
  ├── Shaders/
  │    └── ToxicWater.shader (ShaderLabを用いた2D用波打ちシェーダー)
  └── Materials/
       └── ToxicWaterMat.mat
```

## 3. 実装フェーズと詳細仕様

### フェーズ1: 依存パッケージの確認と設定
*   `manifest.json` を確認・編集し、以下のパッケージが含まれていることを保証してください。
    *   `com.unity.2d.tilemap`
    *   `com.unity.ui`
    *   `com.unity.ads`
*   プロジェクトのピクセルパーフェクト設定のため、カメラのProjectionをOrthographicに設定し、ベースのPPU（Pixels Per Unit）を16として扱います。

### フェーズ2: スクリプトによる自動アセット生成とTilemap構築
Claudeは画像ファイルを直接生成できないため、プレースホルダー用のテクスチャやTileアセットをスクリプトから自動生成（またはエディタスクリプトで生成）するアプローチを取ってください。
*   **Tilemap**: `Grid` の下に `Ground` と `Hazard` (毒の沼) の2つのTilemapを作成するスクリプトを記述してください。
*   `Ground` には `TilemapCollider2D` をアタッチしてください。
*   `Hazard` には `TilemapCollider2D` (IsTrigger = true) をアタッチし、触れるとプレイヤーが死亡する判定を付与してください。

### フェーズ3: プレイヤーコントローラー (PlayerController.cs)
iOSデバイスのタッチ操作（後日実装）やMacでのデバッグを想定し、レスポンスの良い2Dアクション挙動を実装してください。
*   **コンポーネント**: `Rigidbody2D` (KinematicではなくDynamic, Z回転固定), `BoxCollider2D`。
*   **挙動**: 
    *   左右移動（`Input.GetAxisRaw("Horizontal")`を使用し、キビキビとした加減速）。
    *   ジャンプ（接地判定にはレイキャストを使用。ボタン長押しによるジャンプ高の調整は今回は不要だが、拡張しやすい構造にすること）。
*   **イベント**: 
    *   `Hazard` レイヤーのトリガーに侵入した場合、`GameManager` に死亡イベントを通知する。

### フェーズ4: ShaderLabによるカスタムシェーダー (ToxicWater.shader)
2Dのドット絵に適用する「毒の沼」のシェーダーを作成してください。
*   Shader Graphのファイル（.shadergraph）はテキスト生成が困難なため、今回は **ShaderLab (HLSL)** を使用して `.shader` ファイルを生成してください。
*   **要件**:
    *   `_MainTex` を受け取る2Dスプライト用シェーダー。
    *   時間 (`_Time.y`) を用いて、UV座標のY軸をサイン波で歪ませる波打ちエフェクト（UVスクロール＋頂点揺れ）。
    *   色は毒を連想させる紫または緑をベースにし、アルファブレンドをサポートすること。

### フェーズ5: UI ToolkitによるUI実装 (UXML / USS)
`UIManager.cs` と連動するUIを作成してください。
*   **UIStyle.uss**:
    *   ドット絵ゲームに合うような、フラットでコントラストの強いレトロなボタンスタイル。
    *   フォントサイズやマージンをiOS画面でも見やすい比率に設定。
*   **MainMenu.uxml**: タイトルテキストと「START」ボタン。
*   **HUD.uxml**: 画面左上に現在のスコア（または生存時間）を表示。
*   **GameOver.uxml**: 「GAME OVER」テキスト、「RETRY」ボタン、および「動画を見て復活 (REVIVE)」ボタン。

### フェーズ6: 広告マネージャー (AdManager.cs)
Unity Ads SDK (`UnityEngine.Advertisements`) を用いてリワード広告を実装してください。
*   `IUnityAdsInitializationListener`, `IUnityAdsLoadListener`, `IUnityAdsShowListener` を実装すること。
*   **Game ID**: プレースホルダーとして `"1234567"` (iOS) を使用。Test Modeを有効化。
*   **Ad Unit ID**: プレースホルダーとして `"Rewarded_iOS"` を使用。
*   **挙動**: 広告視聴完了 (`UnityAdsShowCompletionState.COMPLETED`) のコールバックを受け取ったら、`GameManager` にプレイヤー復活の処理を叩く。

### フェーズ7: ゲーム進行管理 (GameManager.cs)
全体のステート（タイトル、ゲーム中、ゲームオーバー）を管理するシングルトンクラスを作成してください。
*   プレイヤーの死亡時に `UIManager` を呼び出してゲームオーバー画面を表示。
*   「動画を見て復活」ボタンが押されたら `AdManager` を呼び出し、成功コールバックでプレイヤーの位置を安全な座標に戻してゲームを再開する。

---
**【Claude Codeへの最終指示】**
上記フェーズ1〜7を完全に理解したら、まずは「ディレクトリの作成」と「依存パッケージの確認」から実行し、その後各C#スクリプト、UXML、USS、Shaderファイルを順番に生成してディスクに書き出してください。コードは省略せず、完全に動作する状態で出力してください。
