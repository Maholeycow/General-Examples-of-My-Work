/* Author:     Patrick VanVorce
 * Course:     CS 210
 * Instructor: Mike Wilder
 * Assignment: #1
 * Due Date:   March 10, 2017
 */

#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <stdlib.h>

const int MAX_LENGTH = 256;
const int FILE_NAME_LENGTH = 16;

void processFile(char str[]);
void operator_check(char ch_one, FILE* stream);
void keyword_check(char ch_one, FILE* stream);
void identifier_check();
void comment_check(char ch_one, FILE* stream);
void string_check(char ch_one, FILE* stream);
void character_literal(char ch_one, FILE* stream);
void numeric_literal(char ch_one, FILE* stream);
void unk(char ch_one, FILE* stream);

/* Ragged array list to check for keywords, used in keyword_check */
char *isKeyword[34] = { "accessor", "and", "array", "begin", "bool", "case",
                        "else", "elsif", "end", "exit", "function", "if",
                        "in", "integer", "interface", "is", "loop", "module",
                        "mutator", "natural", "null", "of", "or", "others",
                        "out", "positive", "procedure", "return", "struct",
                        "subtype", "then", "type", "when", "while" };

/* Ragged array list to check for operators, used in operator_check */
char *isOperator[27] = { ".", "<", ">", "(", ")", "+", "-", "*", "/", "|",
                         "&", ";", ",", ":", "[", "]", "=", ":=", "..",
                         "<<", ">>", "<>", "<=", ">=", "**", "!=", "=>" };

int main(void) {

  char fileName[FILE_NAME_LENGTH];
  printf("Please enter the file name: \n");
  scanf("%s", fileName);
  processFile(fileName);

  return 0;
}

/****************************************************
 ** Brains of the operation. Handles what state we **
 ** go to based on a character by character basis  **
 ** using the ASCII table decimal values for       **
 ** characters.                                    **
 ***************************************************/
void processFile(char str[]){
  FILE *fp = fopen(str, "r");
  if(fp == NULL){
    printf("***ERROR: File doesn't exist, terminating...\n");
    exit(1);
  }
  int ch, ch_next;
  char *chartype;
  char str_buffer[256] = "";
  int i = 0;

  while ( fp != 0 && ch != -1){
    ch = fgetc(fp);

    if( ch == '\t' || ch == '\n' )
      continue;

    switch( ch ){
    case 65 ... 90:
    case 97 ... 122: /* A-Z or a-z */
      keyword_check(ch, fp);
      break;
    case 48 ... 57: /* integer number */
      numeric_literal(ch, fp);
      break;
    case 58 ... 62: /* type is operator */
      operator_check(ch, fp);
      break;
    case 40 ... 43: /* type is operator */
      operator_check(ch, fp);
      break;
    case 45 ... 47: /* type is operator */
      operator_check(ch, fp);
      break;
    case 33: /* type is operator */
      operator_check(ch, fp);
      break;
    case 34: /* beginning of a string, double quote */
      string_check(ch, fp);
      break;
    case 39: /* single quote */
      character_literal(ch, fp);
      break;
    case 91: /* type is operator */
      operator_check(ch, fp);
      break;
    case 93: /* type is operator */
      operator_check(ch, fp);
      break;
    case 38: /* type is operator */
      operator_check(ch, fp);
      break;
    default: /*type UNK */
      //unk(ch, fp);
      break;
    }
  }
  fclose(fp);
}

/***********************************************
 *** First, uses one buffer to check if it is **
 *** an operator with two symbols, then       **
 *** checks to see if it is single operator   **
 **********************************************/
void operator_check(char ch_one, FILE* stream){
  char str_buffer_one[2] = "";
  char str_buffer_two[2] = "";
  int i = 17;
  int j = 0, was_complex = 0, was_comment = 0;
  char next_char = fgetc(stream);

  str_buffer_one[0] = ch_one;
  str_buffer_one[1] = next_char;
  str_buffer_one[2] = '\0';

  str_buffer_two[0] = ch_one;
  str_buffer_two[1] = '\0';

  if(ch_one == '/' && next_char == '*'){
    was_comment = 1;
    ungetc(next_char, stream);
    comment_check(ch_one, stream);
  }

  if(was_comment == 0){
    for( i; i < 27; i++ ){
      if((strcmp(str_buffer_one,isOperator[i])) == 0){
        printf("%s (operator)\n", str_buffer_one);
        was_complex = 1;
        break;
      }
    }

    for( j; j < 17; j++){
      if((strcmp(str_buffer_two,isOperator[j])) == 0 && was_complex == 0 ){
        printf("%s (operator)\n", str_buffer_two);
        ungetc(next_char, stream);
        break;
      }
    }
  }
}

/*******************************************
 *** Goes char by char and adds to buffer **
 *** until a non alphabetic character is  **
 *** seen. If not a keyword, then it is an**
 *** identifier.                          **
 ******************************************/
void keyword_check(char ch_one, FILE* stream){
  char str_buffer[256] = "";
  int i = 0, j = 0, is_identifier = 1;

  while(isalpha(ch_one)){
    str_buffer[i++] = ch_one;
    str_buffer[i] = '\0';
    ch_one = fgetc(stream);
  }

  for( j; j<34; j++){
    if(strcmp(str_buffer,isKeyword[j]) == 0
       && ch_one != '_' && isdigit(ch_one) == 0){
      ungetc(ch_one, stream);
      is_identifier = 0;
      printf("%s (keyword)\n", str_buffer);
    }
  }

  if(is_identifier == 1){
    while(isalpha(ch_one) != 0 || isdigit(ch_one) != 0 || ch_one == '_'){
      str_buffer[i++] = ch_one;
      ch_one = fgetc(stream);
    }
    ungetc(ch_one, stream);
    printf("%s (identifier)\n", str_buffer);
  }
}

/***********************************************
 *** Once the beginning of a file is seen,   ***
 *** traverse and store in buffer until end  ***
 *** of comment. Then print it out.          ***
 **********************************************/

void comment_check(char ch_one, FILE* stream){
  char next_char;
  printf("%c", ch_one);
  while(next_char = fgetc(stream)){
    if(next_char == '/' && ch_one == '*'){
      printf("%c (comment)\n", next_char);
      break;
    } else{
      printf("%c", next_char);
    }
    ch_one = next_char;
  }
}

/***********************************************
 *** Goes until another " is seen and stores ***
 *** it in the buffer. Prints.               ***
 **********************************************/
void string_check(char ch_one, FILE* stream){
  char str_buffer[256] = "";
  int i = 0;

  do{
    str_buffer[i++] = ch_one;
    ch_one = fgetc(stream);
  } while(ch_one != 34);
  str_buffer[i++] = ch_one;
  str_buffer[i++] = '\0';

  printf("%s (string)\n", str_buffer);
}

/*******************************************
 *** Prints out the character literal    ***
 ******************************************/
void character_literal(char ch_one, FILE* stream){
  char str_buffer[3] = "";
  int i = 0;

  for(i; i<3; i++){
    str_buffer[i] = ch_one;
    ch_one = fgetc(stream);
  }
  str_buffer[i] = '\0';
  printf("%s (character literal)\n", str_buffer);
}

/*******************************************
 ** Places numeric literal into a string  **
 ** and outputs. Starts with digit, then  **
 ** followed by decimal, A through F or   **
 ** a through f, or '_', '.', and '#'     **
 ******************************************/
void numeric_literal(char ch_one, FILE* stream){
  char str_buffer[256] = "";
  int i = 0;

  str_buffer[i++] = ch_one;
  ch_one = fgetc(stream);
  while(isdigit(ch_one) != 0 || isalpha(ch_one) != 0
        || ch_one == '_' || ch_one == '.' || ch_one =='#'){
    str_buffer[i++] = ch_one;
    ch_one = fgetc(stream);
  }
  str_buffer[i] = '\0';
  printf("%s (numeric literal)\n", str_buffer);
  ungetc(ch_one, stream);
}

/***********************************************
 ** proceeds to place chars into a str buffer **
 ** until a space is found and outputs unk..  **
 **********************************************/
void unk(char ch_one, FILE* stream){
  //DO NOTHING, NO NEED FOR UNK.
}
