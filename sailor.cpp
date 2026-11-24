
#include <iostream>
#include <string>
#include <cstdlib>
#include <ctime>
using namespace std;

class Pirate {
public:
    string name;
    int health;
    int attack;
    int gold;

    Pirate(string n, int a) {
        name = n;
        health = 100;
        attack = a;
        gold = 0;
    }

    void showStatus() {
        cout << "\nCaptain: " << name;
        cout << "\nHealth: " << health;
        cout << "\nGold: " << gold << endl;
    }

    void fight(int &enemyHealth) {
        int choice;

        while (health > 0 && enemyHealth > 0) {
            showStatus();

            cout << "\nEnemy Health: " << enemyHealth;
            cout << "\n1. Normal Attack";
            cout << "\n2. Special Attack";
            cout << "\nChoose your move: ";
            cin >> choice;

            if (choice == 1) {
                enemyHealth -= attack;
                cout << "\nYou attacked the enemy!\n";
            }
            else if (choice == 2) {
                enemyHealth -= attack + 20;
                cout << "\nSPECIAL ATTACK! Amazing power!\n";
            }
            else {
                cout << "\nInvalid choice!\n";
                continue;
            }

            if (enemyHealth > 0) {
                int damage = rand() % 15 + 5;
                health -= damage;
                cout << "Enemy attacked you for "
                     << damage << " damage!\n";
            }
        }
    }
};

int main() {
    srand(time(0));

    string name;
    int choice;
    int power = 20;

    cout << "====================================\n";
    cout << "       THE SAILOR OF THE SEA\n";
    cout << "           ONE PIECE QUEST\n";
    cout << "====================================\n";

    cout << "\nEnter your pirate name: ";
    cin >> name;

    cout << "\nChoose your character:\n";
    cout << "1. Sword Fighter\n";
    cout << "2. Devil Fruit User\n";
    cout << "3. Sniper\n";
    cout << "Choice: ";
    cin >> choice;

    if (choice == 1) {
        power = 25;
    }
    else if (choice == 2) {
        power = 35;
    }
    else if (choice == 3) {
        power = 30;
    }

    Pirate player(name, power);

    cout << "\nWelcome Captain " << name << "!\n";
    cout << "Your journey to find the One Piece begins!\n";

    // First Battle
    cout << "\nA Marine is attacking your ship!\n";

    int marineHealth = 60;
    player.fight(marineHealth);

    if (player.health <= 0) {
        cout << "\nGAME OVER! You were defeated.\n";
        return 0;
    }

    cout << "\nYou defeated the Marine!\n";
    player.gold += 100;

    // Treasure Island
    cout << "\nYou discover Treasure Island!\n";
    cout << "1. Search for treasure\n";
    cout << "2. Continue sailing\n";
    cout << "Choice: ";
    cin >> choice;

    if (choice == 1) {
        player.gold += 500;
        cout << "\nYou found 500 gold coins!\n";
    }

    // Final Boss
    cout << "\n====================================\n";
    cout << "            FINAL BOSS\n";
    cout << "====================================\n";

    cout << "\nA legendary pirate challenges you!\n";

    int bossHealth = 120;
    player.fight(bossHealth);

    if (player.health <= 0) {
        cout << "\nGAME OVER! You lost the battle.\n";
    }
    else {
        cout << "\n====================================\n";
        cout << "       YOU FOUND THE ONE PIECE!\n";
        cout << "====================================\n";

        cout << "\nCongratulations Captain "
             << player.name << "!\n";

        cout << "You are now the PIRATE KING!\n";

        player.showStatus();
    }

    cout << "\nThanks for playing!\n";

    return 0;
}
