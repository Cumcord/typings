# Typedef YAML syntax

## `toplevel`
The `toplevel` key is the string to use for the root namespace.
In this case `'@cumcord'`.

## `decls`
The `decls` keys is a string to insert at the top of the defs file before the generated defs, allowing shared and recursive types for example.

## `defs`
The `defs` key contains a tree representing the defs. This is where the generator does its work, and the rest of this comment will explain how it works:

### Defs: namespaces
each namespace is represented as a string key mapping to a list:
```yml
- myNamespace:
   - subNamespace:
     - subSubNamespace:
       - # etc
```

### Defs: props
each prop is represented by a string tuple:
```yml
- nmspc:
  - [const, 'myCoolExport: string']
  - [function, 'len(x: string): number']
```

### Defs: full imports
full imports (`import * as x from "y"`) are represented with a string tuple:
```yml
- [import, react]
```
They sterilise and prefix the package name to an identifier as so:
- replace the following: `@-./` with `_`
- prefix with `_`

### Defs: custom imports
custom imports are represented with a string threeple:
```yml
- [import, '{ createElement, useState }', react]
```
This is useful in the case that you need to _selectively_ re-export types from a package
while keeping typedoc intact (just exporting consts with a typedef doesnt work).

### Defs: simple exports
simple exports (`export { x };`) are represented with a string tuple:
```yml
- [export, '_react as React']
```
the only real reason these exist is for re-exporing react, which the definitelytyped defs use `export =`, preventing `export *` from working.

### Defs: re-exports
re-exports (`export * as x from "y"`) are represented with a string threeple:
```yml
- [export, ReactDOM, react-dom]
```
This is the main way very common external modules are exported.