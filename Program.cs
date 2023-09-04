using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Xml.Linq;

// 주석에 커스텀은 주어진 코드에서 활용을 위해 변경한 코드입니다.
// 선행 과제 = 게임 분석

// 게임의 룰 (김진환이 이해한 게임의 룰)
// 1. 처음에 카드 2장씩 뽑고 딜러의 첫번째 카드를 뒤집어 놓습니다.
/*
 * 카드뽑기.
 * 딜러의 첫 카드 뽑자마자 뒤집기. 
 */
// 2. 처음 제공된 2장의 카드를 보고 1장더 뽑을지 멈출지를 정합니다.
/*
 * 플레이어의 선택
 * 선택에 따른 진행
 */
// 3. 플레이어가 판단하에 카드 뽑는 것을 멈춘다면 딜러가 카드를 뽑기 시작합니다.
/*
 * 딜러의 턴 구현
 * 토탈 17점 이상일때 멈춤.
 */
// 4. 스톱을 하거나 둘중에 카드의 합이 21을 넘어갈 경우 게임을 멈춥니다.
/*
 * 스톱 체크
 * 버스트 체크
 */
// 5. 승패가 결정됩니다.
/*
 * 승패 결정
 * 버스트 확인
 * 마지막 페이지 출력
 */


// 1. 제공코드 분석 (Prectice 활용)
// 2. 도면 제작 (딜러와 플레이어의 위치 탐색, 덱 그리기등)
//  2-1. 직관적인 게임을 만들기 위해 그림으로 표현
//  2-2. 뽑은 카드는 바로바로 업데이트
// 3. 게임의 룰 구현 (class Blackjack)
// 4. 승리 조건 구현 (Main)


public enum Suit { Hearts, Diamonds, Clubs, Spades }
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

// 카드 한 장을 표현하는 클래스
public class Card
{
    public Suit Suit { get; private set; }
    public Rank Rank { get; private set; }

    public Card(Suit s, Rank r)
    {
        Suit = s;
        Rank = r;
    }

    public int GetValue()
    {
        if ((int)Rank <= 10)
        {
            return (int)Rank;
        }
        else if ((int)Rank <= 13)
        {
            return 10;
        }
        else
        {
            return 11;
        }
    }

    // 커스텀.
    public override string ToString()
    {
        // ◆ ♠ ♣ ♥
        // 간결한 출력을 위한 커스텀 코드
        char sym;
        switch(Suit)
        {
            case Suit.Hearts:
                sym = '♥';
                break;
            case Suit.Diamonds:
                sym = '◆';
                break;
            case Suit.Clubs:
                sym = '♣';
                break;
            case Suit.Spades:
                sym = '♠';
                break;
            default:
                sym = '?';
                break;
        }

        string rank;
        switch (Rank)
        {
            case Rank.Two:
                rank = "2";
                break;
            case Rank.Three:
                rank = "3";
                break;
            case Rank.Four:
                rank = "4";
                break;
            case Rank.Five:
                rank = "5";
                break;
            case Rank.Six:
                rank = "6";
                break;
            case Rank.Seven:
                rank = "7";
                break;
            case Rank.Eight:
                rank = "8";
                break;
            case Rank.Nine:
                rank = "9";
                break;
            case Rank.Ten:
                rank = "10";
                break;
            case Rank.Jack:
                rank = "J";
                break;
            case Rank.Queen:
                rank = "Q";
                break;
            case Rank.King:
                rank = "K";
                break;
            case Rank.Ace:
                rank = "A";
                break;
            default:
                rank = "?";
                break;
        }

        return $"{sym} {rank}";
    }
}

// 덱을 표현하는 클래스
public class Deck
{
    private List<Card> cards;

    public Deck()
    {
        cards = new List<Card>();

        foreach (Suit s in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank r in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(s, r));
            }
        }

        Shuffle();
    }

    public void Shuffle()
    {
        Random rand = new Random();

        for (int i = 0; i < cards.Count; i++)
        {
            int j = rand.Next(i, cards.Count);
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    public Card DrawCard()
    {
        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    // 덱그리기에 활용할 남은 카드 반환 함수.
    public int CardCount()
    {
        return cards.Count;
    }
}

// 패를 표현하는 클래스
public class Hand
{
    private List<Card> cards;

    public Hand()
    {
        cards = new List<Card>();
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public int GetTotalValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            if (card.Rank == Rank.Ace)
            {
                aceCount++;
            }
            total += card.GetValue();
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    // 덮어진 딜러의 첫 카드를 게임이 끝날때 다시 나타내기 위한 내부함수.
    public Card FirstCard()
    {
        return cards[0];
    }
 }

// 플레이어 클래스
public class Player
{
    public Hand Hand { get; private set; }

    // 표시를 위한 이름
    public string Name { get; set; }

    // 뽑은 카드의 그릴 위치를 저장하는 변수.
    public int[] cardPosition = { 0, 0 };

    // 구현된 기능 : 카드 뽑기.
    // 구현할 기능
    // 1. 플레이어 그리기
    // 2. 플레이어의 손패 업데이트 및 그리기

    // 커스텀
    public Player()
    {
        Hand = new Hand();

        // 플레이어의 이름을 입력받아야 하기에 Name은 공백으로.
        Name = "";
        // 플레이어의 카드표시 초기 위치.
        // 노가다의 산물.
        cardPosition[0] = 5;
        cardPosition[1] = 20;
    }

    public Card DrawCardFromDeck(Deck deck)
    {
        Card drawnCard = deck.DrawCard();
        Hand.AddCard(drawnCard);
        return drawnCard;
    }

    // 아래부터 직접 만든 함수 ---------------------------------------
    // 플레이어의 초기 모습을 그립니다.
    public virtual void Drow()
    {
        // 플레이어 그리기
        Console.SetCursorPosition(3, cardPosition[1] - 2);
        Console.Write("{0}", Name);
    }
    // 플레이어의 덱 정보를 업데이트하고 다시 그립니다.
    public virtual void Update(Deck deck)
    {
        // 플레이어의 카드를 그릴 위치로 커서 변경
        Console.SetCursorPosition(cardPosition[0], cardPosition[1]);
        // 카드를 1장 뽑고 그 카드를 패로 가져옵니다.
        Card card = DrawCardFromDeck(deck);
        // 뽑은 카드를 그려줍니다.
        Console.Write(card.ToString());
        // 다음 카드를 그리기 위해 위치를 조금 옮겨줍니다.
        cardPosition[0] += 9;

        // 직관적인 게임 진행을 위해 카드의 합을 그려줍니다.
        Console.SetCursorPosition(3, cardPosition[1] + 2);
        Console.Write("Total Score : {0}", Hand.GetTotalValue());
    }
}

// 플레이어를 상속받은 딜러
public class Dealer : Player
{
    // 구현할 기능
    // 1. 딜러 그리기
    // 2. 플레이어의 손패 업데이트 및 그리기

    // 게임이 끝났을 때를 위 딜러의 첫번째 카드 위치의 X값입니다.
    public int firstCardPositionX;

    // 초기화
    public Dealer() 
    {
        // 딜러의 이름은 고정이므로 생성시 이름을 정합니다.
        Name = "Dealer";
        // Player와 마찬가지로 카드를 그릴 위치를 정하지만 플레이어와 위치가 다르므로 조정해줍니다.
        cardPosition[0] = 5;
        cardPosition[1] = 4;
        firstCardPositionX = cardPosition[0];
    }

    // 오버라이드
    public override void Update(Deck deck)
    {
        // 플레이어와 같은 코드부분입니다.
        Console.SetCursorPosition(cardPosition[0], cardPosition[1]);
        Card card = DrawCardFromDeck(deck);
        Console.Write(card.ToString());
        cardPosition[0] += 9;

        // 뒤집어진 카드 때문에 출력문이 조금 다르게 설정되어 만들어진 코드입니다.
        Console.SetCursorPosition(3, cardPosition[1] + 2);
        Console.Write("Total Score : {0}", Hand.GetTotalValue() - Hand.FirstCard().GetValue());
    }

}

// 블랙잭 게임을 구현하세요. 
public class Blackjack
{
    // 블랙잭 1판
    // 기능 : 카드 드로우, 스탑, 끝나는 조건, 판 그리기
    public int[] deckSize = { 10, 7 };
    public int[] startDeckDrow = { 20,10 };
    public int[] deckTextPositon = { 0,0 };

    Player player;
    Dealer dealer;
    Deck deck;

    // 초기화 해줍시다.
    public Blackjack(Player player, Dealer dealer)
    {
        // 플레이어랑 딜러의 포인터를 받아옵니다.
        this.player = player;
        this.dealer = dealer;
        // 덱은 아예 새로 생성해줍시다.
        deck = new Deck();
        // 이 위치는 덱의 위치에 따라 바뀌어야 하기 때문에 초기화시 설정해줍니다.
        deckTextPositon[0] = startDeckDrow[0] + 1;
        deckTextPositon[1] = startDeckDrow[1] + 0;
    }

    // 덱을 그리거나 플레이어, 딜러의 플레이공간을 그립니다.
    public void DrowBox(int startPointX, int startPointY, int width, int height)
    {
        for (int i = 0; i < width - 2; i++)
        {
            Console.SetCursorPosition(startPointX + i, startPointY - 1);
            Console.Write("-");
            Console.SetCursorPosition(startPointX + i, startPointY + height - 2);
            Console.Write("-");
        }
        for (int i = 0; i < height - 2; i++)
        {
            Console.SetCursorPosition(startPointX - 1, startPointY + i);  // 7
            Console.Write("|");
            Console.SetCursorPosition(startPointX + width - 2, startPointY + i); // 16
            Console.Write("|");
        }
    }

    // 플레이어와 딜러 사이에 놓일 덱을 그립니다.
    // ==> 일종의 표시공간입니다.
    public void DrowFeild()
    {
        DrowBox(startDeckDrow[0], startDeckDrow[1], deckSize[0], deckSize[1]);
        DrowBox(1, player.cardPosition[1] - 2, 50, 7);
        DrowBox(1, dealer.cardPosition[1] - 2, 50, 7);

        Console.SetCursorPosition(deckTextPositon[0]+1, deckTextPositon[1] + 1);
        Console.Write("Deck");

        Console.SetCursorPosition(deckTextPositon[0] + 2, deckTextPositon[1] + 3);
        Console.Write("{0}", deck.CardCount());
    }
    // 덱에 그려진 정보를 전부 지웁니다.
    public void DeckTextDelete()
    {
        for(int i = 0; i < deckSize[1] - 2; i++)
        {
            Console.SetCursorPosition(deckTextPositon[0], deckTextPositon[1] + i);
            Console.Write("       ");
        }
    }
    // 플레이어의 이름을 입력받습니다.
    public void SetPlayerName()
    {
        DeckTextDelete();

        // 시작 화면시 덱 내부에 적힐 글.
        Console.SetCursorPosition(deckTextPositon[0], deckTextPositon[1] + 1);
        Console.Write("Input");
        Console.SetCursorPosition(deckTextPositon[0], deckTextPositon[1] + 2);
        Console.Write("Your");
        Console.SetCursorPosition(deckTextPositon[0], deckTextPositon[1] + 3);
        Console.Write("Name");

        // 유저의 이름을 받아들이기.
        Console.SetCursorPosition(deckTextPositon[0]+10, deckTextPositon[1] + 3);
        string playerName = Console.ReadLine();
        // 적힌 이름 콘솔창에서 지우기.
        Console.SetCursorPosition(deckTextPositon[0]+10, deckTextPositon[1] + 3);
        Console.Write("     ");

        // 덱내부의 텍스트 지우기
        DeckTextDelete();

        // 덱 내부에 정보 입력.
        Console.SetCursorPosition(deckTextPositon[0] + 1, deckTextPositon[1] + 1);
        Console.Write("Deck");

        Console.SetCursorPosition(deckTextPositon[0] + 2, deckTextPositon[1] + 3);
        Console.Write("{0}", deck.CardCount());

        if (playerName == null || playerName == " ")
            player.Name = "Player1";
        player.Name = playerName;
    }

    
    // 플레이시 항상 덱에 남은 카드의 수를 업데이트합니다.
    public void DeckCountUpdate()
    {
        Console.SetCursorPosition(deckTextPositon[0], deckTextPositon[1] + 3);
        Console.Write("       ");

        Console.SetCursorPosition(deckTextPositon[0] + 2, deckTextPositon[1] + 3);
        Console.Write("{0}", deck.CardCount());
    }

    // 각각의 선수가 카드를 뽑는 과정입니다.
    public void UpdateProcces(string name)
    {
        if(name == "player")
        {
            player.Update(deck);
            DeckCountUpdate();
            Thread.Sleep(1000);
        }
        else if(name == "dealer")
        {
            dealer.Update(deck);
            DeckCountUpdate();
            Thread.Sleep(1000);
        }

    }

    // 처음 시작시 고정으로 실행되는 코드입니다.
    public void SetStart()
    {
        // 플레이어와 딜러를 그립니다.
        player.Drow();
        dealer.Drow();

        // 2장씩 뽑고 딜러는 1장 뒤집어 놓습니다. -----------
        UpdateProcces("player");

        // 이 부분은 딜러의 카드를 1장 덮어놓기 위해 만들어졌습니다.
        dealer.Update(deck);
        Console.SetCursorPosition(dealer.firstCardPositionX, dealer.cardPosition[1]);
        Console.Write("-??-");
        DeckCountUpdate();
        Thread.Sleep(1000);

        UpdateProcces("player");
        UpdateProcces("dealer");
    }

    // 게임이 끝났을 경우 표시되는 모든 부분에 대한 코드입니다.
    public void EndPage(int winner, bool isBust)
    {
        // 딜러의 카드를 공개하는 시간입니다.
        Console.SetCursorPosition(dealer.firstCardPositionX, dealer.cardPosition[1]);
        Console.Write(dealer.Hand.FirstCard().ToString());
        Console.SetCursorPosition(3, dealer.cardPosition[1] + 2);
        Console.Write("Total Score : {0}", dealer.Hand.GetTotalValue());

        // 마지막으로 승패의 결과를 출력합니다.
        Console.SetCursorPosition(0, startDeckDrow[1] + 1);
        string end = isBust ? "!!Bust!!" : "!!End!!";
        Console.WriteLine("--{0}--", end);
        Console.WriteLine();

        // 스위치문을 통해 승자에 따라 다르게 출력합니다.
        switch (winner)
        {
            case 0:
                Console.WriteLine("비겼습니다.", player.Hand.GetTotalValue());
                break;
            case 1:
                Console.WriteLine("{0}의 승리!", player.Name);
                break;
            case 2:
                Console.WriteLine("{0}의 승리!", dealer.Name);
                break;
        }

        // 마지막 유예시간을 줍니다.
        Console.SetCursorPosition(0, player.cardPosition[1] + 4);
        Console.ReadLine();
    }

    // 승패의 조건중 하나인 버스트의 조건을 체크합니다.
    public bool CheckBust()
    {
        if(player.Hand.GetTotalValue() > 21 || dealer.Hand.GetTotalValue() > 21)
            return true;

        return false;
    }

    // 유저에게 선택지를 제시하기위한 텍스트를 그리는 함수입니다.
    public void ChoiceText(bool isOk)
    {
        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1]);
        Console.Write("               ");
        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1]);
        Console.Write("1. 한장 더!!!");

        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 1);
        Console.Write("               ");
        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 1);
        Console.Write("2. 스톱!!!");

        // 입력이 잘못되었을 때를 위한 예외처리입니다.
        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 3);
        Console.Write("                             ");
        if(!isOk)
        {
            Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 3);
            Console.Write("?!?! 잘못 입력하였습니다 ?!?!");
        }


        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 4);
        Console.Write("              ");
        Console.SetCursorPosition(startDeckDrow[0] + deckSize[0] + 3, startDeckDrow[1] + 4);
        Console.Write(" => ");
    }

}

class Program
{
    static void Main(string[] args)
    {
        // 초기값 설정
        Player player = new Player();
        Dealer dealer = new Dealer();
        Blackjack blackjack = new Blackjack(player, dealer);

        // 게임 진행을 위한 변수.
        int winner = 0;
        bool isStop = false;
        bool isBust = false;
        bool isOK = true;

        // 블랙잭 초기 화면 그리기.
        blackjack.DrowFeild();
        blackjack.SetPlayerName();
        blackjack.SetStart();

        // 게임시작 플레이어의 차례
        while (!isStop)
        {
            if (!isStop)
            {
                // 플레이어의 선택 화면 출력
                blackjack.ChoiceText(isOK);
                string choice = Console.ReadLine();

                // 선택에 따른 구현
                // 1. 한장 더 뽑기
                // 2. 스톱
                isOK = true;
                if (choice == "1")
                {
                    blackjack.UpdateProcces("player");
                }
                else if (choice == "2")
                {
                    isStop = true;
                }
                else
                {
                    isOK = false;
                    continue;
                }
            }

            // 카드의 합이 21이 넘는 경우 Bust
            if (blackjack.CheckBust())
            {
                if (player.Hand.GetTotalValue() > 21)
                {
                    winner = 2;
                }

                isBust = true;
                break;
            }
        }

        // 딜러의 차례
        // 플레이어가 버스트일 경우 무조건 딜러의 승.
        if (!isBust)
        {
            // isStop의 재활용
            isStop = false;

            // 딜러는 카드의 합이 17이상일 경우까지 계속 뽑기.
            while (!isStop)
            {
                if (dealer.Hand.GetTotalValue() < 17)
                    blackjack.UpdateProcces("dealer");
                else
                {
                    isStop = true;
                }

                if (blackjack.CheckBust())
                {
                    winner = 1;
                    isBust = true;
                    break;
                }
            }
        }


        // 버스트가 아닐경우 승자를 정해주어야 합니다.
        if(!isBust)
        {
            // winner = 0   무승부
            // winner = 1   플레이어 승리
            // winner = 2   딜러 승리
            if (dealer.Hand.GetTotalValue() > player.Hand.GetTotalValue())
            {
                winner = 2;
            }
            else if(dealer.Hand.GetTotalValue() < player.Hand.GetTotalValue())
            {
                winner = 1; 
            }
            else
            {
                winner = 0;
            }
        }

        // 마지막 페이지를 출력합니다. (덱 왼쪽)
        blackjack.EndPage(winner, isBust);
    }
}