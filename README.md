# Huffman-coding-compression
#### Compression & decompression of text files using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding).

## Usage
To compress or decompress files, pass the paths to the files as parameters to the program. You can do that by simply dragging and dropping the selected files on the program, and Windows will automatically pass the paths to the files as arguments to the program.

When a file filename.txt is passed to the program, it is compressed and the bytes are written to a file  filename.txt.comp, created in the same location as filename.txt is. 

When a file filename.txt.comp is passed to the program, it is decompressed and the text is written to a file filename.txt, created in the same location is filename.txt.comp is.

## Inner workings
### Text encoding
Both compression and decompression uses ISO-8859-2 (/latin 2) encoding.

### Compression method
Each character is assigned an alternative code (instead of the latin-2 8-bit one) using [huffman coding](https://en.wikipedia.org/wiki/Huffman_coding). Higher occuring characters simply have shorter codes and less occuring characters longer codes, which most of the time results in smaller total amount of bits. This implementation operates on individual characters, it does not encode sections of text (which would give better compression results).

This specific implementation compresses files to about 70% of their initial size, saving 30% of space.
### Storing of the tree
Informations about each character are stored as follows:

`[bit indicating if the character was already encountered in the text]`,

if not: `[length of the character's code]`, `[the character's latin-2 representation]`, `[the character's code]`

if yes: `[the character's code]`

- the character's code length is represented in 3-6 bits (which means that the maximal code length is 2^6 - 1 = 63 - so in this implementation there's a limit on the amount of different characters that can be present in the text - but it is very unlikely than any text would generate code larger than 63 bits). How does the decompressor know how many bits are representing the code's length? In the beginning of the file, the first two bytes indicate exactly that. 00 is for 3 bits, 01 for 4 bits, 10 for 5 bits and 11 for 6 bits.

- Informations about each characters are not written at the beginning or the end of the file, but directly whenever the character is in the text (the only information written directly at the beginning of the file - even before the redundant zeros - is a two-bit indicator of in how many bits the codes lengths are stored).

- The bits are stored in bytes, and each byte has 8 bits. If the amount of bits isn't divisible by 8 without a remainder, additional bits must be added. This implementation adds zeros at the beginning of the compressed text, and because the first information on each character is if it has (0) or has not (1) already been encountered, then the first bit on the first character must obviously always be 1. This allows the decompressor to safely recognize and ignore the redundant bits -> all zeros before the first non-zero character.

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

## license
MIT

## To-do
- Storing each bit (while compressing and mainly decompressing) as one character was an absolutely retarded idea. This way, representing one bit takes 2 bytes (the size of one character), which is unacceptable. It's needed to, for example, create a BitArray or BitList class, which will internally store a collection of bytes, and will write (and read) each bit to a byte. This way representing one bit will take 1/8 byte (+ the size of instantiating the class - the internal collection, fields etc.), which is a significant improvement.

- Limit the amount of threads that are used to concurrently compress/decompress files (either set a limit based on the PC's processor, or use ThreadPool)

- Optimize speed
