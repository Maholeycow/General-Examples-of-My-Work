#ifndef SYMTBL_H
#define SYMTBL_H

#define NSYMS   (3)
//comment
struct sym {
  char * name;
  double value;
  struct sym *next;
}*root;
//*head;

struct sym * sym_lookup(char *s);
void print_syms();
void add_constants();
struct sym* SortedMerge(struct sym* a, struct sym* b);
void FrontBackSplit(struct sym* source, struct sym** frontRef, struct sym** bac\
kRef);
void MergeSort(struct sym** headRef);
#endif /* SYMTBL_H */

