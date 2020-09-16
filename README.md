
# Robocube Language

## Introduction

RCL is a high-level language designed to meet the needs of modern financial
decision-makers.

* Eliminates tedious loop programming.
* Native support for tabular data (cubes) and query syntax.
* Simplified syntax to express async, concurrent and parallel workflows.

RCL is a strong, dynamically-typed, interpreted language which strives to
continue in the traditions of LISP and APL.

Like LISP, RCL is homoiconic, which means that all RCL programs are also valid
RCL values or "data." This property makes it is easy to write code that manipulates
code, and it is easy to convert all types of RCL values to and from a canonical
RCL text syntax, which is equally human and machine readable.

Like APL, RCL encourages iteration abstraction, which spares the programmer
from tedious and error-prone loop coding. In fact, there are no scalar types at
all in RCL. Data values are always held in a vector. RCL uses APL-style
"right-to-left" operator evaluation, with no precedence among operators to
provide a consistent coding experience across the entire language.

    RCL>3 * 9 - 2
    21

Unlike both LISP and APL, RCL provides a modern, JSON-like syntax, free of any
nested parentheses or obscure mathematical symbols.

RCL was developed in a world of asynchronous applications and workflows.
As a result, the approach to asynchrony in RCL is unique to RCL. In most
programming languages, subroutines execute syncronously by default, and async
execution has to be explicitly requested by the developer using specialized
syntax. In contrast, RCL enables any operator to complete evaluation
asyncronously without programmer intervention.

Syntactically, it is as simple to make an http request and wait for the response...

    RCL>response:getw "https://mycompany.com/api/v1/mythings"

as it is to take the sum of a vector:

    RCL>result:sum 1 2 3 4 5

## Syntax

### Blocks

A block is an ordered collection of names and values.

    RCL>{x:1 y:20 z:300}
    {
      x:1
      y:20
      z:300
    }

Blocks are used to represent data records:

    RCL>{first_name:"brian" last_name:"programmer" url:"https://twitch.tv/brianprogrammer"}
    {
      first_name:"brian"
      last_name:"programmer"
      url:"https://twitch.tv/brianprogrammer"
    }

Blocks are also used to represent code blocks:

    RCL>{x:1 y:20 z:$x + $y}
    {
      x:1
      y:20
      z:21
    }

#### Evaluation of Blocks

Code blocks can be eval'd using the eval operator:

    RCL>eval {x:1 y:20 z:$x + $y}
    {
      x:1
      y:20
      z:21
    }

The token between the name and the value is called an "Evaluator." Evaluators
can alter the behavior of a value when the block is evaluated.

Let (:) evaluates the value on the right and places the result in the named
variable:

    RCL>eval {z:1 + 20}
    {
      z:21
    }

Yield (&lt;-) evaluates the value on the right and makes it the result of
evaluation for the block:

    RCL>eval {<-1 + 20}
    21

Quote (::) skips evaluation of the value on the right and places the value in
the named variable:

    RCL>eval {x::$y + $z}
    {
      x:$y + $z
    }

This feature is useful for writing macros that combine dynamically and
statically generated operator expressions.

For complete control of the body of generated code blocks, two more evaluators
are provided; these evalutors combine the behavior of yield with the behavior
of let and quote respectively:

Yield-Quote (&lt;-:) skips evaluation of the value on the right and makes it the
right-hand value of a Yield expression in the result:

    RCL>eval {x:1 y:20 <-:$x + $y}
    {
      x:1
      y:20
      <-$x + $y
    }

Yield-Eval (&lt;-:) evaluates the the value on the right and makes it the
right-hand value of a Yield expression in the result:

    RCL>eval {<--20 + 1}
    {
      <-21
    }

By default, a block nested within another block will not evaluate automatically
when its parent is evaluated. When defining an operation to be invoked
elsewhere, it is not necessary to suppress evaluation using :: or &lt;-: in
this case.

		RCL>eval {k:{x:1 y:2 <-$x + $y} <-k {}}
		3

In the previous case, the block `k` must be treated like an operator in order
to trigger evaluation. In order to trigger evaluation at the point where the
block is defined, use `eval`.

    RCL>eval {k:eval {x:1 y:2 <-$x + $y} <-$k}
    3

#### Names defined within blocks

Names defined within a block should be valid c-style identifiers, that is, they
should begin with a letter or underscore and contain only letters, numbers, or
underscores. When it is not possible to use valid c-style identifiers, names
can be single-quoted:

    RCL>{'name with spaces':1}
    {
      'name with spaces':1
    }

    RCL>{'-name#with#special#chars':1}
    {
      '-name#with#special#chars':1
    }

If you must, the single quote can also be escaped:

    RCL>{'\'name with single quotes\'':1}
    {
      '\'name with single quotes\'':1
    }

### References

In RCL, references are values which contain the name given to another value
within a block or a cube. References are denoted with a $-prefix followed by
the variable name.

    RCL>x:1
    RCL>$x
    1

References can point to a value nested within a block, using dot notation to
separate the nested variable names.

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

References are only dereferenced when they are evaluated.

#### Reference Scope

RCL uses lexical scoping to evaluate references. Wherever there is a block,
there is a scope.

When evaluating references, RCL always searches backwards and then up. The
first matching variable name is used.

The value is found in the same scope as the reference.

    RCL>eval {x:1 <-$x + $x}
    2

The value is defined in the enclosing scope, but overridden in the current scope.

    RCL>eval {x:10 <-eval {x:1 <-$x + $x}}
    2

The value is found in an enclosing scope.

    RCL>eval {x:10 <-eval {<-$x + $x}}
    20

The referenced value is nested within a block found in an enclosing scope.

    RCL>eval {numbers:{one:1 two:2 three:3} <-$numbers.one + $numbers.three}
    4

#### Overriding the root scope with eval

By default, RCL will search for a value in every enclosing scope until it gets
to the root scope. If greater isolation is desired, a clean root scope can be
defined by passing a block into the left operand of `eval`.

    RCL>x:10
    RCL>{} eval {<-$x + $x}
    2020.09.14 21:12:33.096883 0 0 fiber 0 failed
      --- BEGIN STACK (bot:0,fiber:0,lines:4) ---
      L:{}
      R:{<-$x + $x}
      -- {} eval {<-$x + $x}
      -- $x + $x
      --- END STACK ---
      --------------------------------------------------------------------------------
      Unable to resolve name x
      --------------------------------------------------------------------------------

The variable `$x` will only resolve successfully if it is explicitly defined in
the scope passed to eval.

    RCL>x:10
    RCL>{x:100} eval {<-$x + $x}
    200

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
operators `date`, `daytime`, `datetime`, `timestamp` and `timespan` convert
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
references. Syntactically, references to operations are identical to variable
references without the $-prefix.

#### Object-based Operations

It is possible to implement primitive objects by enclosing "member" variables
within a block that also contain defined operations.

    RCL>o:{x:10 f:$x * $x}
    RCL>o.f {}
    100

So long as the invocation of the operation contains the name of the block to
which the operation belongs, all of the variables contained within `o` will be
visible to the operation.

### Templates

Templates are a way to compose arbitrary text fragments in RCL. Similar to
"here docs" in other languages, templates are used to produce complex,
multiline documents.

Suppose we have the following content in a file templates.rcl:

    t:[?
      this is where "[! $x !]" will appear!
    ?]

At the command line:

    RCL>templates:parse load "templates.rcl"
    RCL>x:"x is a string"
    RCL>templates.t {}
    "this is where "x is a string" will appear!\n"

The result of evaluating a template is always a string. For a more presentable
result, convert the string back into a template.

#### Iteration within Templates

RCL's vectorized operators excel at composing more complex text documents.
Suppose we have following content in templates.rcl:

    multiline:[?
      <xml>
        [! "<elem>" + $x + "</elem>\n" !]
      </xml>
    ?]

Then, at the RCL command line:

    RCL>templates:parse load "repro.rcl"
    RCL>x:"elem1" "elem2" "elem3"
    RCL>templates.multiline {}
    "<xml>\n  <elem>elem1</elem>\n  <elem>elem2</elem>\n  <elem>elem3</elem>\n</xml>\n"
    RCL>template templates.multiline {}
    [?
      <xml>
        <elem>elem1</elem>
        <elem>elem2</elem>
        <elem>elem3</elem>
      </xml>
    ?]

Notice that indentation is always controlled by the placement of the `[!` token
on the line.

#### Template scoping and evaluation rules

The scoping rules for references found within a template are identical to those
for blocks.

Evaluation for a template always results in a string that represents the
composed text. Evaluation can be controlled for templates in the same way as
for blocks. For example:

    RCL>h:"hello"
    RCL>w:"world"
    RCL>eval [?--[! $h !] [! $w !]--?]
    "--hello world--"

Alternatively, a template can behave like an operation.

    RCL>h:"hello"
    RCL>w:"world"
    RCL>t:[?--[! $h !] [! $w !]--?]
    RCL>t {}
    "--hello world--"

Like all operations, operations implemented as templates can utilize the
special variables $R and $L to access left and right operands:

    RCL>t:[?--[! $R !] [! $L !]--?]
    RCL>"hello" t "world"
    "--world hello--"

#### Template escaping rules

Sometimes it is useful to use a template to generate rcl syntax which might
include an escaped template.

    RCL>t:[??[?--[!! $L !!] [!! $R !!]--?]??]
    RCL>"hello" t "world"
    "[?--hello world--?]"

Notice the resulting template is valid RCL synax and can be parsed and eval'd like any rcl value.

    RCL>parse "hello" t "world"
    [?--hello world--?]
    RCL>eval parse "hello" t "world"
    "--hello world--"

### Cubes

Cubes are RCL's native tabular data structure. Cubes are intended to facilate
advanced query and data analysis operations within RCL without the assistance
of additional tools, infrastructure or api's.

Simple cubes can function like a multidimensinal array:

    RCL>[x y z 1 2 3 4 5 6 7 8 9]
    [
       x  y  z
       1  2  3
       4  5  6
       7  8  9
    ]

Operators can be applied to the columns of a cube, to produce a new cube with a
column of results:

    RCL>u:[a b c 1 2 3 4 5 6 7 8 9]
    RCL>$u.a + $u.b
    [
       x
       3
       9
      15
    ]

The first column of the resulting cube is always named `x`.

Use the operator `cube` to construct a new cube with control over the column
names, order and content. In this case it is necessary to explicitly evaluate
the block containing the column info.

    RCL>u:[a b c 1 2 3 4 5 6 7 8 9]
    RCL>$u
    [
       a  b  c
       1  2  3
       4  5  6
       7  8  9
    ]
    RCL>cube eval {d:$u.a + $u.b e:$u.b * $u.c}
    [
       d  e
       3  6
       9 30
      15 72
    ]

In contrast to vectors, cubes allow for missing data values.

    RCL>u:[x y z 1 -- -- -- 1 -- -- -- 1]
    RCL>$u
    [
       x  y  z
       1 -- --
      --  1 --
      -- --  1
    ]

Missing data values can be replaced with a default value using the plug
operator:

    RCL>0 plug $u
    [
       x  y  z
       1  0  0
       0  1  0
       0  0  1
    ]

And default values can be removed using the unplug operator:

		RCL>u:[x y z 1 0 0 0 1 0 0 0 1]
		RCL>$u
		[
			 x  y  z
			 1  0  0
			 0  1  0
			 0  0  1
		]
		RCL>0 unplug $u
		[
			 x  y  z
			 1 -- --
			--  1 --
			-- --  1
		]

The rows of a cube can be indexed using special column `S` which contains a
symbol for every row.

		RCL>u:[S|price #abc 10.00 #def 100.00 #ghi 35.00]
		RCL>$u
		[
			S   |price
			#abc  10.0
			#def 100.0
			#ghi  35.0
		]

Using the `S` column simplifies the process of combining data sets into a
single cube:

    RCL>u:[S|price #abc 10.00 #def 100.00 #ghi 35.00]
    RCL>v:[S|volume #ghi 10000 #abc 200000 #def 25000 #klm 30000]
    RCL>$u ! $v
    [
      S   |price volume
      #abc  10.0 200000
      #def 100.0  25000
      #ghi  35.0  10000
      #klm    --  30000
    ]

