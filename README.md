WorldFileResolver
=================

経緯度と地図画像ピクセル座標のセット、及び地図投影系定義からワールドファイルを生成します。

機能
----

投影系が判っていてマッピングポイントのセットもある絵地図画像の、ワールドファイルを計算で生成します。  
[ワールドファイル](http://en.wikipedia.org/wiki/World_file)が判らない人には多分不要の代物です。

QGISのgeoreferencerでできるじゃんとか言わない。  
既にマッピングポイントセットがある時にコマンドラインだけで使えるのと、複数の投影系を振って計算中途の座標値で分散を取って、正しい投影系を推定するためのライブラリを作るベースとして作成。

使い方
------

Mac OS X /w Monoの場合

```
mono WorldFileResolver.exe [-f fromProjText|-g fromepsgCode] [-t toProjText|-u toEpsgCode] [-o outputFile] inputFile
```

Windowsで動くかは未確認。

* inputFile

経緯度、絵地図ピクセル座標のセット(4カラム)の羅列です。  
1,2カラム目は基本WGS84ですが、-f/-gオプションで他の投影系にも変換できます。  
3,4カラム目はピクセル座標です。  
ピクセル座標であって、-t/-uオプションで指定した投影系ではないので注意。

例：

```
135.7528889, 34.75, 1286.858385, 805.0550064
135.8778889, 34.66666667, 8416.95973, 6564.0437
135.7528889, 34.66666667, 1288.436191, 6581.399556
135.8778889, 34.75, 8401.181679, 798.7437859
```

* -t/-u オプション

絵地図の投影系を何とみなすかを指定します。  
inputFileの1,2カラム目指定をWGS84等の経緯度で行う場合は、基本必要です。  
絵地図/古地図がこの投影系で描かれているものと扱って、ピクセル座標との間でワールドファイルを計算します。

1,2カラム目の座標指定が既に絵地図の投影系で指定されている場合は、 -t/-u オプション、-f/-g オプションともに不要です。  
その場合は、ワールドファイル計算前の投影変換を行わないので、そのままの座標値が処理に渡されます。

-tと-uの使い分けは、投影系定義をprojTextで渡すか、EPSGコードで渡すかで使い分けてください。

* -f/-g オプション

inputFileの1,2カラム目の座標系を何とみなすかを指定します。  
無指定の場合は、WGS84としてみなし、-t/-uで指定の投影系に変換後、ワールドファイル推定します。

省略時はWGS84経緯度と見なされますし、絵地図の投影座標系で直接指定する場合は、-t/-u オプション、-f/-g オプションともに不要ですから、基本的には本オプションが必要になるケースは滅多にありません。  
入力経緯度の値を旧日本測地系で行いたい場合等のためのオプションです。

* -o オプション

計算結果のワールドファイルパラメータをファイルに出力します。  
無指定の場合は、標準出力に結果が返されます。

例
--

上記の例のinputFileで、

```
mono WorldFileResolver.exe -f "+proj=longlat +ellps=bessel +towgs84=-146.336,506.832,680.254,0,0,0,0 +no_defs" -t "+proj=utm +zone=53 +ellps=bessel +towgs84=-146.336,506.832,680.254,0,0,0,0 +units=m +no_defs" inputFile
```

した結果は、

```
1.60722900381249
0.0103653767194359
0.0105581030890587
-1.60117918973334
566829.270401219
3846475.92510777
```

になります。

今後
----

あまりガシガシ発展させるつもりで作ったわけではありませんが、投影法の候補を指定してどの投影法が正しいか判定する機能等も気が向けばつけるかもしれません。

ライセンス
----------

MIT
