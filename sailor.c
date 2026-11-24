
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

int main() {
    char name[50];
    int choice, attack, enemyHealth;
    int health = 100;
    int gold = 0;

    srand(time(NULL));

    printf("====================================\n");
    printf("       THE SAILOR OF THE SEA\n");
    printf("          ONE PIECE QUEST\n");
    printf("====================================\n");

    printf("\nWelcome, future Pirate King!\n");
    printf("Enter your pirate name: ");
    scanf("%49s", name);

    printf("\nWelcome Captain %s!\n", name);

    printf("\nChoose your fighting style:\n");
    printf("1. Sword Fighter\n");
    printf("2. Devil Fruit User\n");
    printf("3. Sniper\n");

    printf("Enter choice: ");
    scanf("%d", &choice);

    if (choice == 1) {
        attack = 25;
        printf("\nYou chose Sword Fighter!\n");
    }
    else if (choice == 2) {
        attack = 35;
        printf("\nYou chose Devil Fruit User!\n");
    }
    else if (choice == 3) {
        attack = 30;
        printf("\nYou chose Sniper!\n");
    }
    else {
        attack = 20;
        printf("\nYou chose Pirate Fighter!\n");
    }

    printf("\nYour adventure begins on the Grand Line!\n");

    // First Battle
    printf("\nA Marine appears in front of you!\n");
    enemyHealth = 60;

    while (enemyHealth > 0 && health > 0) {
        printf("\nYour Health: %d", health);
        printf("\nEnemy Health: %d", enemyHealth);

        printf("\n1. Attack");
        printf("\n2. Run Away");
        printf("\nChoose: ");
        scanf("%d", &choice);

        if (choice == 1) {
            enemyHealth -= attack;
            printf("\nYou attacked the Marine!\n");

            if (enemyHealth > 0) {
                health -= 15;
                printf("Marine attacks you! -15 HP\n");
            }
        }
        else if (choice == 2) {
            printf("\nYou escaped from battle!\n");
            break;
        }
        else {
            printf("\nInvalid choice!\n");
        }
    }

    if (health <= 0) {
        printf("\nGAME OVER! You were defeated.\n");
        return 0;
    }

    if (enemyHealth <= 0) {
        printf("\nYou defeated the Marine!\n");
        gold += 100;
    }

    // Treasure Island
    printf("\nYou discover a mysterious island!\n");
    printf("1. Search for treasure\n");
    printf("2. Continue sailing\n");
    printf("Choose: ");
    scanf("%d", &choice);

    if (choice == 1) {
        printf("\nYou found 500 gold coins!\n");
        gold += 500;
    }
    else {
        printf("\nYou continue your journey.\n");
    }

    // Final Boss
    printf("\n====================================\n");
    printf("       FINAL BATTLE\n");
    printf("====================================\n");

    printf("\nA powerful pirate appears!\n");
    printf("You must defeat the enemy to find the One Piece!\n");

    enemyHealth = 120;

    while (enemyHealth > 0 && health > 0) {
        printf("\nYour Health: %d", health);
        printf("\nBoss Health: %d", enemyHealth);

        printf("\n1. Normal Attack");
        printf("\n2. Special Attack");
        printf("\nChoose: ");
        scanf("%d", &choice);

        if (choice == 1) {
            enemyHealth -= attack;
            health -= 10;
            printf("\nYou performed a normal attack!\n");
        }
        else if (choice == 2) {
            enemyHealth -= attack + 20;
            health -= 15;
            printf("\nYou used your SPECIAL POWER!\n");
        }
        else {
            printf("\nInvalid choice!\n");
        }
    }

    if (health <= 0) {
        printf("\nGAME OVER! The enemy defeated you.\n");
    }
    else {
        printf("\nCONGRATULATIONS CAPTAIN %s!\n", name);
        printf("YOU FOUND THE ONE PIECE!\n");
        printf("You are now the PIRATE KING!\n");
        printf("Gold collected: %d\n", gold);
    }

    printf("\nThanks for playing The Sailor of the Sea!\n");

    return 0;
}
