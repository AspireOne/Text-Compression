# Huffman-coding-compression
Compression & decompression of text files using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding)

# Inner workings
#### Encoding
Both compression and decompression uses ISO-8859-2 (/latin 2) encoding.

#### Compression
Each character is assigned an alternative code (instead of the latin-2 8-bit one) using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding). Higher occuring characters have shorter codes and less occuring characters longer codes, which most of the time results in smaller total amount of bits.
