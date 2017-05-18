%{
#include <malloc.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "sym.h"
  double pi = 3.14159;
  double phi = 1.61803;
  %}

%union {
  double dval;
  struct sym * symptr;
}

%token <symptr> NAME
%token <dval> NUMBER
%left '-' '+'
%left '*' '/'
%nonassoc UMINUS

%type <dval> expression
%%
statement_list
: statement '\n'
| statement_list statement '\n'
;

statement
: NAME '=' expression {if(strcmp($1->name, "PI")==0){printf("assign to const\n"\
); $1->value = pi;} else if(strcmp($1->name, "PHI")==0){printf("assign to const\
\n"); $1->value = phi;} else  $1->value = $3; }
| expression { printf("= %g\n", $1); }
;

expression
: expression '+' expression { $$ = $1 + $3; }
| expression '-' expression { $$ = $1 - $3; }
| expression '*' expression { $$ = $1 * $3; }
| expression '/' expression { if($3==0){$$ = $1; printf("divide by zero\n");} e\
lse{$$ = $1 / $3;} }
| '-' expression %prec UMINUS { $$ = -$2; }
| '(' expression ')' { $$ = $2; }
| NUMBER
| NAME { if(!strcmp("PHI", $1->name)){$$ = phi;} else if(!strcmp("PI", $1->name\
)){$$ = pi;} else $$ = $1->value; }
;

%%

struct sym * sym_lookup(char * s)
{
  char * p;
  struct sym * sp;
  static int x = 0;
  /*Start the list so we can enter for loop below*/
  if(root==NULL){
    root = (struct sym*)malloc(sizeof(struct sym));
    root->next=NULL;
    root->name=strdup(s);
  }

  for (sp=root; sp!=NULL; sp=sp->next)
    {
      /* is it already here? */
      if(sp->name && !strcmp(sp->name, s)){
        return sp;
      }

      /* is it free */
      if(sp->next==NULL) {
        sp->next = (struct sym*)malloc(sizeof(struct sym));
        sp = sp->next;
        sp->next=0;
        sp->name=strdup(s);
        if(!strcmp("PI", s)){
          sp->value=pi;
        }
        if(!strcmp("PHI", s)){
          sp->value=phi;
        }
        return sp;
      }
      /*otherwise continue to next*/
    }

  //yyerror("Too many symbols");
  exit(-1);
  return NULL; /* unreachable */
}

void print_syms()
{
  printf("Entered print_syms function...\n");
  struct sym* conductor;
  //printf("Root is %s\n", conductor->name);
  MergeSort(&root);
  int count = 0;
  conductor=root;
  while(conductor!=NULL)
    {
      count++;
      conductor=conductor->next;
    }
  conductor = root;
  printf("num-syms: %d\n", count);
  while(conductor!=NULL)
    {
      if(!strcmp(conductor->name, "PHI")){
        printf("        %s => %f\n", conductor->name, phi);//****
      } else
        printf("        %s => %g\n", conductor->name, conductor->value);//****
      conductor=conductor->next;
    }
}

/* sorts the linked list by changing next pointers (not data) */
void MergeSort(struct sym** headRef)
{
  struct sym* head = *headRef;
  struct sym* a;
  struct sym* b;

  /* Base case -- length 0 or 1 */
  if ((head == NULL) || (head->next == NULL))
    {
      return;
    }

  /* Split head into 'a' and 'b' sublists */
  FrontBackSplit(head, &a, &b);
  /* Recursively sort the sublists */
  MergeSort(&a);
  MergeSort(&b);

  /* answer = merge the two sorted lists together */
  *headRef = SortedMerge(a, b);
}

struct sym* SortedMerge(struct sym* a, struct sym* b)
{
  struct sym* result = NULL;

  /* Base cases */
  if (a == NULL)
    return(b);
  else if (b==NULL)
    return(a);

  /* Pick either a or b, and recur */
  if (strcmp(a->name, b->name) <= 0)
    {
      result = a;
      result->next = SortedMerge(a->next, b);
    }
  else
    {
      result = b;
      result->next = SortedMerge(a, b->next);
    }
  return(result);
}

/* Split the nodes of the given list into front and back halves,
     and return the two lists using the reference parameters.
     If the length is odd, the extra node should go in the front list.
     Uses the fast/slow pointer strategy.  */
void FrontBackSplit(struct sym* source,
                    struct sym** frontRef, struct sym** backRef)
{
  struct sym* fast;
  struct sym* slow;
  if (source==NULL || source->next==NULL)
    {
      /* length < 2 cases */
      *frontRef = source;
      *backRef = NULL;
    }
  else
    {
      slow = source;
      fast = source->next;

      /* Advance 'fast' two nodes, and advance 'slow' one node */
      while (fast != NULL)
        {
          fast = fast->next;
          if (fast != NULL)
            {
              slow = slow->next;
              fast = fast->next;
            }
        }

      /* 'slow' is before the midpoint in the list, so split it in two
         at that point. */
      *frontRef = source;
      *backRef = slow->next;
      slow->next = NULL;
    }
}


