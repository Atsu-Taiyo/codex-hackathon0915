# WebGL text and icons

Room One explicitly loads `Resources/Fonts/NotoSansCJKjp-Regular` for its IMGUI labels, including Japanese language selection. The font importer must retain `includeFontData: 1`; WebGL cannot rely on installed system fonts.

Font source: https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/Japanese/NotoSansCJKjp-Regular.otf

The adjacent `Resources/Fonts/LICENSE.txt` contains the SIL Open Font License shipped with the font.

The bundled font is subset with fontTools to Latin U+0020–024F, punctuation U+2000–206F, kana U+3000–30FF, and the kanji used in the language selector (`動詞選英語`). When adding Japanese copy, extend this subset as needed. This keeps the Unity data file below the hosting upload limit.

`RoomOneIcons` renders play, home, star, reset, arrows, drag handles, and busy indicators from antialiased geometric masks. These symbols no longer depend on font glyph availability. The same drawing path is used by the workshop and collection views.
