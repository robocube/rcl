
# Robocube Language

## Introduction

RCL is a high-level language designed specifically to meet the needs of modern
financial decision-makers.

* Eliminaties tedious loop programming.
* Native support for tabular data (cubes) and query syntax.
* Simplified syntax to express async, concurrent and parallel workflows.

RCL is a strong-typed, dynamically-typed, interpreted language which strives to
continue in the traditions of LISP and APL.

Like LISP, RCL is homoiconic, which means that all RCL programs are also valid
RCL values or "data." This property makes it is easy to write code that manipulates
code, and it is easy to convert all types of RCL values to and from a canonical
RCL text syntax, which is equally human and machine readable.

Like APL, RCL encourages iteration abstraction, which spares the programmer
from tedious and error-prone loop coding. In fact, there are no scalar types at
all in RCL. Data values are always held in a vector. RCL uses APL-style
"right-to-left" operator evaluation, with no precedence among operators to
provide a consistent coding experience across all operators.

    :3 * 9 - 2
    21 (not 25)

Unlike both LISP and APL, RCL provides a modern, JSON-like syntax, free of any
nested parentheses or obscure mathematical symbols.

RCL was developed in a world of asynchronous applications and workflows.
As a result, the approach to asynchrony in RCL is unique to RCL. In most
programming languages, subroutines execute syncronously by default, and async
execution has to be explicitly requested by the developer using specialized
syntax. In contrast, RCL enables any operator to complete evaluation
asyncronously without programmer intervention.

Syntactically, it is as simple to make an http request and wait for the response...

    response:getw "https://mycompany.com/api/v1/mythings"

as it is to take the sum of a vector:

    result:sum 1 2 3 4 5

## Syntax

### Blocks

A block is an ordered collection of names and values.

    {x:1 y:20 z:300}

Blocks are used to represent data records:

    {first_name:"brian" last_name:"programmer" url:"https://twitch.tv/brianprogrammer"}

Blocks are also used to represent code blocks:

    {x:1 y:20 z:$x + $y}

#### Evaluation of Blocks

Code blocks can be eval'd using the eval operator:

    eval {x:1 y:20 z:$x + $y} -> {x:1 y:20 z:21}

The token between the name and the value is called an "Evaluator." Evaluators
can alter the behavior of the value when the block is evaluated.

Let (:) evaluates the value on the right and places the result in the named
variable:

    eval {z:1 + 20} -> {z:21}

Yield (&lt;-) evaluates the value on the right and makes it the result of evaluation:

    eval {<-1 + 20} -> 21

Quote (::) skips evaluation of the value on the right and places the value in
the named variable:

    eval {x::$y + $z} -> {x:$y + $z}

This feature is useful for writing macros that combine dynamically and
statically generated operator expressions.

For complete control of the body of generated code blocks, two more evaluators
are provided; these evalutors combine the behavior of yield with the behavior
of let and quote respectively:

Yield-Quote (&lt;-:) skips evaluation of the value on the right and makes it the
right-hand value of a Yield expression in the result:

    eval {x:1 y:20 <-:$x + $y} -> {x:1 y:20 <-$x + $y}

Yield-Eval (&lt-:) evaluates the the value on the right and makes it the
right-hand value of a Yield expression in the result:

    eval {<--20 + 1} -> {<-21}

#### Names defined within blocks

Names defined within a block should be valid c-style identifiers, that is, they
should begin with a letter or underscore and contain only letters, numbers, or
underscores. When it is not possible to use valid c-style identifiers, names
can be single-quoted:

    {'name with spaces':1}

    {'-name#with#special#chars':1}

### References

In RCL, variable references are denoted with a $-prefix followed by the variable name:

    RCL>x:1
    RCL>$x
    1

References can point to a value within a block:

    RCL>k:{x:1 y:2 z:3}
    RCL>$k.z
    3

References can also point to a column within a cube:

    RCL>u:[S|x y #a 1 10 #b 2 20 #c 3 30]
    RCL>$u
    [
      S | x  y
      #a  1 10
      #b  2 20
      #c  3 30
    ]
    RCL>$u.x
    [
      S | x
      #a  1
      #b  2
      #c  3
    ]
    RCL>$u.y
    [
      S | y
      #a 10
      #b 20
      #c 30
    ]

Variables names only dereferenced when they are evaluated.

### Vectors

In RCL all concrete data types are always contained within a vector which is an
unnamed, ordered list of values of one of the following scalar types. In
general, the syntax and behavior of the scalar types is similar to C#, the
implementation language of RCL.

In a vector, the scalar values are simply separated by spaces with no
additional adornments or enclosures. For example:

    double_vector:1.0 2.0 3.0
    long_vector:1 2 3
    date_vector:2020.09.13 2020.09.14 2020.09.15
    symbol_vector:#x #y #z

Mixing different types of scalars within a single vector will generate an
error.

#### Scalar Types

##### Double

Doubles are 64-bit floating-point numbers. Doubles are recognized by RCL using
the presence of the decimal point. Any number without a decimal point will be
interpreted as an integer value by default.

##### Long

Longs are 64-bit integer values. Any numeric value without a decimal point will
be interpreted as a long.

##### Boolean

Boolean variables can have only one of two values: `true` or `false`.

##### String

The string type implements c-style string literals. Strings are double quoted,
special characters are escaped using the backslash.

    :"this is a string"
    :"this is a \"string\" within a string"

##### Time

Time scalars in RCL consist of both an integer-based time value (number of
ticks) and a "TimeType", which is one of Date, Daytime, Datetime, Timestamp or
Timespan. The TimeType determines how the time value appears in output as well
as its interpretation in various operators that manipulate time values. The
operators `date`, `daytime`, `datetime`, `timestamp` and `timepspan` convert
time scalars to the chosen TimeType.

    RCL>dt:now {}
    RCL>$dt
    2020.09.13 22:03:33.397974
    RCL>date $dt
    2020.09.13
    RCL>daytime $dt
    22:03:33
    RCL>datetime $dt
    2020.09.13 22:03:33
    RCL>timestamp $dt
    2020.09.13 22:03:33.397974
    RCL>timespan $dt
    737680.22:03:33.397974

##### Symbol

Symbols are tuples combining zero or more scalar values of the other types.

Symbol scalars are always prefixed with a hash sign. All by itself, the hash
sign represents an empty symbol:

    #

A simple symbol with a string value. The string is a valid identifier.

    #valid_id

It does not need to be surrounded with quotes.

    #'not valid id'

Multi-part tuples are expressed by putting commas between the values:

    #id,1,2.0,false,2020.11.03

##### Incr

A special value used to provide row-level instructions for data held within a cube.

Valid incr scalars currently include `++` and `+-`

### Operators

RCL operators are the only way of processing data values in rcl. Operators
utilize traditional infix notation, that is, the operator name is placed between
the left and right operands, for example:

    RCL>1 + 1
    2

As a matter of semantics, operators must always receive a right-hand argument:

    RCL>not true
    false

Some operators support both monadic (single operand) and dyadic (dual operand)
usage. For example the monadic version of the + operator will compute a rolling
sum:

    RCL>+ 1 2 3
    1 3 6

The dyadic version, given a scalar and a vector, will add the scalar to each
element of the vector.

    RCL>1 + 1 2 3
    2 3 4

The dyadic version, given two vectors with equal counts, will perform a
"columnar" addition.

    RCL>1 2 3 + 4 5 6
    5 7 9

#### Operator Nesting

Operators may be nested in order to form a complex expression. There is no
precedence among operators in RCL. Like other APL-based languages, the order of
operations always proceeds from the right-most operator in a complex expression,
towards the left.

So, the following:

    RCL>13 * 7 - 2
    65

RCL will first perform `7 - 2 -> 5`, and then `13 * 5 -> 65` whereas many other
languages will give "precedence" to the * operator over the + operator. So: `13
* 7 -> 91` and then `91 - 2 -> 89`.

In RCL, ALL expressions are ALWAYS interpreted from right to left. There are NO
exceptions. However, the order operator of operations can always be controlled
directly with use of parentheses, for example:

    RCL>(13 * 7) - 2
    89

It is considered best to write expressions in the form that minimizes
parentheses.

#### Operations

Named routines written in RCL are known as "operations", as opposed to
"operators" which are always written in C#. Syntactically, operations are
invoked the same way as operators, that is, with either one or two arguments.
But unlike operators, operations must be defined within the referenceable scope
of the call site where the operator is invoked. When defining an operator, the
special variables `$L` and `$R` can be used to access the left and right
operands.

    RCL>add:{<-$L + $R}
    RCL>1 add 2
    3

#### Namespaced Operations

It is possible to group related operations into a referencable namespace
implemented as a block, and then use dot notation to access them.

    myops:{add:{<-$L + $R} subtract:{<-$L - $R}
    RCL>myops:{add:{<-$L + $R} subtract:{<-$L - $R}}
    RCL>$myops
    {
      add:{
        <-$L + $R
      }
      subtract:{
        <-$L - $R
      }
    }
    RCL>1 myops.add 2
    3
    RCL>1 myops.subtract 2
    -1

In general, scoping and access rules for operations are identical to those for
references.

### Cubes

Cubes are RCL's native tabular data structure. Cubes are intended to facilate
advanced query and data analysis operations within RCL without the assistance
of additional tools, infrastructure or api's.


### Templates

