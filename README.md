# Huffman-coding-compression
#####Compression & decompression of text files using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding).

## Inner workings
### Text encoding
Both compression and decompression uses ISO-8859-2 (/latin 2) encoding.

### Compression method
Each character is assigned an alternative code (instead of the latin-2 8-bit one) using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding). Higher occuring characters simply have shorter codes and less occuring characters longer codes, which most of the time results in smaller total amount of bits. This implementation operates on individual characters, it does not encode sections of text (which would give better compression results).

This specific implementation compresses files to about 70% of their initial size.
### Storing of the tree
Informations about each character are stored as follows:

`[bit indicating if the character was already encountered in the text]`,
if not: `[length of the character's code]`, `[the character's latin-2 representation]`, `[the character's code]`
if yes: `[the character's code]`

- the character's code length is stored in 4 bits (which means that the maximal code length is 2^4 - 1 = 15 - so in this implementation there's a limit on the amount of different characters that can be present in the text).

- Informations about each characters are not written at the beginning or the end of the file, but directly whenever the character is in the text.

- The only information added to a character that was already encountered is one bit (0), which indicates that it has already been encountered. Decompressor then starts reading from the first bit and gradually adds next, until it finds a match in whenever it stores already encountered codes and their latin-2 representations.

example:
<pre>
letter | code | code length | latin-2 code |
   R     1010    4 (0100)       01001010
</pre>

- if not already encountered:
`[1][0100][01001010][1010]` (in the file written together, like this: `10100010010101010`)
- if already encountered:
`[0][1010]` (in the file written together, like this: `01010`)

## Usage
To compress or decompress files, pass the paths to the files as parameters to the program. You can do that by simply dragging and dropping the selected files on the program, and Windows will automatically pass the paths to the files as arguments to the program.

When a file filename.txt is passed to the program, it is compressed and the bytes are written to a file  filename.txt.comp, created in the same location as filename.txt is. 

When a file filename.txt.comp is passed to the program, it is decompressed and the text is written to a file filename.txt, created in the same location is filename.txt.comp is.

