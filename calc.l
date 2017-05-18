%{
/*#include <math.h> */
#include "y.tab.h"
#include "sym.h"
%}

%%
([0-9]+|([0-9]*\.[0-9]+)([eE][+-]?[0-9]+)?) {
    yylval.dval = atof(yytext);
    return NUMBER;
    }

[ \t] ; /* ignore whitespace */

[A-Za-z][A-Za-z0-9]* {
    /* return symbol pointer */
    /*yylval is the value associated with token, whatever that means*/
    /*yytext is the pointer to matched string*/
    /*sym_lookup is a function that takes a string and is of type struct sym * \
...*/
    /*symptr is in y.tab.h and is a char sym * type*/
    static int x = 0;
    if(x==0){
       yylval.symptr = sym_lookup("PHI");
       yylval.symptr = sym_lookup("PI");
       x=1;
    }
    yylval.symptr = sym_lookup(yytext);
    return NAME;
    }
"?\n" { print_syms(); }
"$" { return 0; }
\n |
. { return yytext[0]; };
%%
int yywrap() { return 1; }

