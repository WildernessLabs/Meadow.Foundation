# FontBase - Font Base Class

The `Font` base class defines the properties and methods that must be implemented when defining fonts for ise with the `GraphicsLibrary`.

## API

## Properties

### `int Width`

Width of a character in pixels.

### `int Height`

Height of a character in pixels.

#### `byte[] this[char character]`

The indexer property looks up the character in the internal font table.  If the character is found then the indexer returns a `byte` array that defines the pixel layout of the character.