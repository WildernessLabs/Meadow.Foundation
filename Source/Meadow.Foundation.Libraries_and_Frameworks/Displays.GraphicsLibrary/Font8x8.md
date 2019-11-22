# Font8x8 - Defines the characters in the 8x8 font

## API

### Properties

#### `int Width`

Width of a character in pixels.

#### `int Height`

Height of a character in pixels.

#### `byte[] this[char character]`

The indexer property looks up the character in the internal font table.  If the character is found then the indexer returns a `byte` array that defines the pixel layout of the character.