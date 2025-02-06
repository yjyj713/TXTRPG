using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

//인벤토리 내에서 아이템 장착 구현이 안돼요.....
//장착이 안돼요 
namespace Txt_RPG
{
    internal class Program
    {
        public interface ICharacter
        {
            string Name { get; }
            int Health { get; set; }
            int Attack { get; }
            bool IsDead { get; }
            void TakeDamage(int damage);
        }

        public class Warrior : ICharacter //Warrior
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public int AttackPower { get; set; }
            public int Level { get; set; }
            public int Defense { get; set; } //방어력
            public int Gold { get; set; }
            public bool IsDead => Health <= 0;
            public int Attack => new Random().Next(10, AttackPower);
            public List<IItem> Inventory { get; set; } //인벤토리
            public IItem EquippedWeapon { get; set; }// 무기
            public IItem EquippedArmor { get; set; } // 방어구
            public Warrior(string name)
            {
                Name = name;
                Level = 1;
                Health = 100;
                AttackPower = 10;
                Defense = 5;
                Gold = 1500;
                Inventory = new List<IItem>();
                EquippedWeapon = null; //초기화
                EquippedArmor = null; //초기화
            }

            public void TakeDamage(int damage)
            {
                Health -= damage;
                if (IsDead)
                    Console.WriteLine($"{Name}이(가) 죽었습니다.");
                else
                    Console.WriteLine($"{Name}이(가) {damage}의 데미지를 받았습니다. 남은 체력 : {Health}");
            }
        }

        public class Monster : ICharacter //몬스터
        {
            public string Name { get; }
            public int Health { get; set; }
            public int Attack => new Random().Next(10, 20);
            public bool IsDead => Health <= 0;

            public Monster(string name, int health)
            {
                Name = name;
                Health = health;
            }

            public void TakeDamage(int damage)
            {
                Health -= damage;
                if (IsDead)
                    Console.WriteLine($"{Name}이(가) 죽었습니다.");
                else
                    Console.WriteLine($"{Name}이(가) {damage}의 데미지를 받았습니다. 남은 체력 : {Health}");
            }
        }

        public class Goblin : Monster //몬스터 : 고블린
        {
            public Goblin(string name) : base(name, 50) { }
        }

        public class Dragon : Monster //몬스터 : 드래곤
        {
            public Dragon(string name) : base(name, 100) { }
        }

        public interface IItem //아이템 인터페이스 정의
        {
            string Name { get; }
            void Use(Warrior warrior);
            void Equip(Warrior warrior);
        }

        public class Weapon : IItem //아이템 : 무기클래스
        {
            public string Name { get;  set; }
            public string Type { get;  set; }

            public Weapon(string name, string type)
            {
                Name = name;
                Type = type;
            }

            public void Use(Warrior warrior) { }

            public void Equip(Warrior warrior)
            {
                warrior.EquippedWeapon = this;
            }
        }
        public class Armor : IItem  // 아이템 : 방어구 클래스
        {
            public string Name { get; private set; }

            public Armor(string name)
            {
                Name = name;
            }

            public void Use(Warrior warrior) { }

            public void Equip(Warrior warrior)
            {
                warrior.EquippedArmor = this;  
            }
        }

        public interface Reward //보상 인터페이스 정의
        {
            string Name { get; }
            void Use(Warrior warrior);
        }

        public class HealthPotion : Reward //아이템 : 체력 포션 클래스
        {
            public string Name => "체력 포션";

            public void Use(Warrior warrior)
            {
                Console.WriteLine("체력 포션을 사용합니다. 체력이 50 증가합니다.");
                warrior.Health += 50;
                if (warrior.Health > 100) warrior.Health = 100;
            }
        }

        public class StrengthPotion : Reward // 아이템 : 공격력 포션 클래스
        {
            public string Name => "공격력 포션";

            public void Use(Warrior warrior)
            {
                Console.WriteLine("공격력 포션을 사용합니다. 공격력이 10 증가합니다.");
                warrior.AttackPower += 10;
            }
        }

        public class Stage //스테이지 클래스
        {
            private ICharacter player;
            private ICharacter monster;
            private List<Reward> rewards;

            public delegate void GameEvent(ICharacter character);
            public event GameEvent OnCharacterDeath;

            public Stage(ICharacter player, ICharacter monster, List<Reward> rewards)
            {
                this.player = player;
                this.monster = monster;
                this.rewards = rewards;
                OnCharacterDeath += StageClear;
            }

            public void Start()
            {
                Console.WriteLine($"스테이지 시작!\n플레이어의 체력 : {player.Health}, 플레이어의 공격력 : {player.Attack}");
                Console.WriteLine($"몬스터{monster.Name}의 체력 : {monster.Health}, 몬스터{monster.Name}의 공격력 : {monster.Attack}");
                Console.WriteLine("-----------------------------------------------------------------------------------------------");

                while (!player.IsDead && !monster.IsDead)
                {
                    Console.WriteLine($"{player.Name}의 턴");
                    monster.TakeDamage(player.Attack);
                    Console.WriteLine();
                    Thread.Sleep(1000);

                    if (monster.IsDead) break;

                    Console.WriteLine($"{monster.Name}의 턴");
                    player.TakeDamage(monster.Attack);
                    Console.WriteLine();
                    Thread.Sleep(1000);
                }

                if (player.IsDead)
                    OnCharacterDeath?.Invoke(player);
                else if (monster.IsDead)
                    OnCharacterDeath.Invoke(monster);
            }

            private void StageClear(ICharacter character) //스테이지 클리어
            {
                if (character is Monster)
                {
                    Console.WriteLine($"스테이지 클리어 {character.Name}을(를) 물리쳤습니다.");

                    if (rewards != null)
                    {
                        Console.WriteLine("아래의 보상 아이템 중 하나를 선택하여 사용할 수 있습니다 : ");
                        foreach (var item in rewards)
                        {
                            Console.WriteLine(item.Name);
                        }

                        Console.WriteLine("사용할 아이템 이름을 입력하세요 :");
                        string input = Console.ReadLine();

                        Console.WriteLine($"{input} 을(를) 사용했습니다.");
                        Reward selectedItem = rewards.Find(item => item.Name == input);
                        if (selectedItem != null)
                        {
                            selectedItem.Use((Warrior)player);
                        }
                    }

                    player.Health = 100;
                }
                else
                {
                    Console.WriteLine("게임 오버. 패배했습니다");
                }
            }
        }

        public enum Action //던전 전 활동 enum
        {
            Condition = 1,
            Inventory = 2,
            Store = 3,
            Dungeon = 4
        }

        static void ShowCondition(Warrior player) //상태보기창
        {
            Console.WriteLine("상태 보기\n캐릭터의 정보가 표시됩니다.");
            Console.WriteLine($"Lv. {player.Level:D2}");
            Console.WriteLine($"{player.Name} ({typeof(Warrior).Name})");
            Console.WriteLine($"공격력 : {player.AttackPower}");
            Console.WriteLine($"체력: {player.Health}");
            Console.WriteLine($"Gold : {player.Gold} G");
            Console.WriteLine("0. 나가기");

            Console.Write("\n원하시는 행동을 입력해주세요.\n>>");
            string input = Console.ReadLine();
            if (input == "0")
            {
                Console.WriteLine("상태보기 창을 닫습니다.");
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }
        }

        static void ShowInventory(Warrior player) //인벤토리창
        {
            Console.WriteLine("인벤토리\n보유 중인 아이템을 관리할 수 있습니다.\n\n[아이템 목록]");
            int index = 1;

            if (player.Inventory.Count == 0) Console.WriteLine("보유한 아이템이 없습니다.");
            else
            {
                foreach (var item in player.Inventory)
                {
                    string equippedMarker = string.Empty;
                    if (item == player.EquippedWeapon) equippedMarker = ($"[E]{player.EquippedWeapon}");
                    if (item == player.EquippedArmor) equippedMarker = ($"[E]{player.EquippedArmor}");

                    Console.WriteLine($"{index}. {equippedMarker} {item.Name}");
                    index++;
                }
            }
            Console.WriteLine("\n1. 장착 관리");
            Console.WriteLine("0. 나가기");

            Console.Write("\n원하시는 행동을 입력해주세요.\n>> ");
            string input = Console.ReadLine();

            if (input == "1")
            {
                EquipItem(player);
            }
            else if (input == "0")
            {
                Console.WriteLine("인벤토리 창을 닫습니다.");
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }
        }

        static void EquipItem(Warrior player) //인벤토리 내부 구현. 여기가 문제. 
        {
            if (player.Inventory.Count == 0)
            {
                Console.WriteLine("장착할 아이템이 없습니다.");
                return;
            }

            Console.WriteLine("장착할 아이템을 선택하세요:");

            int index = 1;
            foreach (var item in player.Inventory)
            {
                string equippedMarker = string.Empty;
                if (item == player.EquippedWeapon) equippedMarker = ($"[E]{player.EquippedWeapon}");
                if (item == player.EquippedArmor) equippedMarker = ($"[E]{player.EquippedArmor}");

                Console.WriteLine($"{index}. {equippedMarker} {item.Name}");
                index++;
            }

            string input = Console.ReadLine();

            if (int.TryParse(input, out int selectedItemIndex) && selectedItemIndex > 0 && selectedItemIndex <= player.Inventory.Count)
            {
                IItem selectedItem = player.Inventory[selectedItemIndex - 1];

                if (selectedItem == player.EquippedWeapon)
                {
                    Console.WriteLine($"{selectedItem.Name}은(는) 이미 장착된 무기입니다.");
                }
                else if (selectedItem == player.EquippedArmor)
                {
                    Console.WriteLine($"{selectedItem.Name}은(는) 이미 장착된 방어구입니다.");
                }
                else
                {
                    // 무기 장착
                    if (selectedItem is Weapon)
                    {
                        selectedItem.Equip(player); 
                        Console.WriteLine($"{selectedItem.Name}을(를) 장착했습니다.");
                    }
                    // 방어구 장착
                    else if (selectedItem is Armor)
                    {
                        selectedItem.Equip(player);  
                        Console.WriteLine($"{selectedItem.Name}을(를) 장착했습니다.");
                    }
                }
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }
        }


        public interface IStoreItem //상점아이템 인터페이스 정의
        {
            string Name { get; set; }
            int Price { get; }  
            bool isBought { get; set; }
            void Buy(Warrior player);
        }

        public class Stock : IStoreItem , IItem//아이템 : 재고 클래스
        {
            public string Name { get; set; }
            public bool isBought { get; set; }
            public int Price { get; }

            public Stock(string name, int price)
            {
                Name = name;
                Price = price;
                isBought = false;
            }


            public void Use(Warrior warrior)
            {
                warrior.EquippedWeapon = this;
                Console.WriteLine($"{this.Name}을(를) 장착했습니다.");
            }

            public void Equip(Warrior warrior)
            {
                warrior.EquippedArmor = this;

                Console.WriteLine($"{this.Name}을(를) 장착했습니다.");
            }
            public void Buy(Warrior player)
            {
                if (isBought)
                {
                    Console.WriteLine($"{Name}은 이미 구매되었습니다.");
                }
                else
                {
                    if (player.Gold >= Price)
                    {
                        player.Gold -= Price;
                        player.Inventory.Add();
                        isBought = true;
                        Console.WriteLine($"{Name}을(를) 구매했습니다.");
                    }
                    else
                    {
                        Console.WriteLine("골드가 부족합니다.");
                    }
                }
            }
        }

        static void ShowStore(Warrior player) //상점창
        {
            Console.WriteLine("상점\n필요한 아이템을 얻을 수 있는 상점입니다.\n");
            Console.WriteLine($"[보유골드]\n{player.Gold}G");
            Console.WriteLine("\n[아이템 목록]");

            List<IStoreItem> storeItems = new List<IStoreItem>
            {
                new Stock("수련자 갑옷", 1000),
                new Stock("무쇠갑옷", 2000),
                new Stock("스파르타의 갑옷", 3500),
                new Stock("낡은 검", 600),
                new Stock("청동 도끼", 1500),
                new Stock("스파르타의 창", 2400)
            };

            foreach (var item in storeItems)
            {
                Console.WriteLine($"- {storeItems.IndexOf(item) + 1} {item.Name} | 가격: {item.Price}G");
            }

            Console.WriteLine("\n구매할 아이템 번호를 입력해주세요 (0: 나가기)");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int selectedItemIndex) && selectedItemIndex > 0 && selectedItemIndex <= storeItems.Count)
            {
                var selectedItem = storeItems[selectedItemIndex - 1];
                selectedItem.Buy(player);
            }
            else if (input == "0")
            {
                Console.WriteLine("상점을 나갑니다.");
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }
        }

        static void Main(string[] args) //메인
        {
            Console.WriteLine("스파르타 마을에 오신 것을 환영합니다.");
            Console.WriteLine("캐릭터의 이름을 입력해주세요:");
            string playerName = Console.ReadLine();

            Warrior player = new Warrior(playerName);

            Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");

            bool isRunning = true;

            while (isRunning)
            {
                Console.Write("\n1. 상태 보기\n2. 인벤토리\n3. 상점\n4. 던전 입장\n\n원하시는 행동을 입력해주세요.\n>>");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int actionInput) && Enum.IsDefined(typeof(Action), actionInput))
                {
                    Action selectedAction = (Action)actionInput;

                    switch (selectedAction)
                    {
                        case Action.Condition:
                            ShowCondition(player);
                            break;

                        case Action.Inventory:
                            ShowInventory(player);
                            break;

                        case Action.Store:
                            ShowStore(player);
                            break;

                        case Action.Dungeon:
                            Console.WriteLine("던전에 입장합니다.");

                            Goblin goblin = new Goblin("Goblin");
                            Dragon dragon = new Dragon("Dragon");

                            List<Reward> stage1Rewards = new List<Reward> { new HealthPotion(), new StrengthPotion() };
                            List<Reward> stage2Rewards = new List<Reward> { new HealthPotion(), new StrengthPotion() };

                            Stage stage1 = new Stage(player, goblin, stage1Rewards);
                            stage1.Start();

                            if (player.IsDead)
                            {
                                Console.WriteLine("게임 오버");
                                isRunning = false;
                                break;
                            }

                            Stage stage2 = new Stage(player, dragon, stage2Rewards);
                            stage2.Start();

                            if (player.IsDead)
                            {
                                Console.WriteLine("게임 오버");
                                isRunning = false;
                                break;
                            }

                            Console.WriteLine("축하합니다! 모든 스테이지를 클리어했습니다.");
                            break;

                        default:
                            Console.WriteLine("잘못된 선택입니다.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다. 1, 2, 3, 4 중 하나를 입력해주세요.");
                }
            }

            Console.ReadLine();
        }
    }
}
