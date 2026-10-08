# PROJETO: CITYSKYCODE
## Simulador de Cidade Programável para Aprender C#, POO, Arquitetura e Engenharia de Software

Quero desenvolver um jogo de construção e simulação de cidades 3D inspirado na experiência de jogos como **Cities: Skylines**, porém com uma diferença fundamental:

> **O jogador não apenas constrói a cidade. Ele programa a cidade.**

O jogo deve combinar:

- construção de cidades;
- simulação urbana;
- cidadãos;
- veículos;
- trânsito;
- economia;
- energia;
- água;
- serviços públicos;
- felicidade da população;
- problemas urbanos;
- mapas e camadas de visualização;
- gerenciamento territorial;
- eventos;
- objetivos;
- programação em C#;
- Programação Orientada a Objetos;
- arquitetura de software;
- debugging;
- testes;
- refatoração;
- aprendizagem progressiva.

O objetivo não é criar um jogo onde o jogador resolve exercícios de programação desconectados.

O objetivo é criar um **simulador urbano real no qual o código escrito pelo jogador possui consequências reais no mundo do jogo**.

---

# 1. VISÃO DO PRODUTO

Imagine a seguinte experiência:

O jogador começa com:

- um terreno;
- uma estrada;
- algumas casas;
- poucos cidadãos;
- uma prefeitura;
- dinheiro limitado.

O jogo apresenta:

> "Sua cidade precisa crescer."

O jogador poderá construir manualmente algumas estruturas, mas os comportamentos e sistemas deverão progressivamente ser controlados através de código.

Por exemplo:

```csharp
var casa = new Casa();

casa.Capacidade = 4;
casa.ConsumoEnergia = 2;

cidade.Construir(casa);
```

A casa realmente deverá aparecer no mundo.

Depois:

```csharp
var trabalhador = new Trabalhador();

trabalhador.Salario = 2500;

empresa.Contratar(trabalhador);
```

O cidadão deverá existir.

Ele deverá:

- morar;
- trabalhar;
- receber salário;
- gastar dinheiro;
- utilizar serviços;
- deslocar-se;
- possuir necessidades;
- contribuir para a economia;
- afetar a cidade.

O código não pode ser apenas "teatro".

Ele precisa alterar efetivamente a simulação.

---

# 2. OBJETIVO PRINCIPAL

O jogador deve terminar o jogo sabendo não apenas escrever código, mas entendendo:

```text
Fundamentos de C#
        ↓
Programação Orientada a Objetos
        ↓
Modelagem de domínio
        ↓
Composição
        ↓
Interfaces
        ↓
Herança
        ↓
Polimorfismo
        ↓
SOLID
        ↓
Design Patterns
        ↓
Arquitetura
        ↓
Testes
        ↓
Refatoração
        ↓
Sistemas complexos
```

A cidade é o laboratório de programação.

---

# 3. REFERÊNCIA DE EXPERIÊNCIA

A experiência de construção deve lembrar um city builder moderno.

O jogador deverá poder:

- ampliar o mapa;
- construir ruas;
- conectar ruas;
- criar cruzamentos;
- criar bairros;
- definir zonas;
- construir residências;
- construir comércio;
- construir indústria;
- criar serviços;
- administrar energia;
- administrar água;
- administrar esgoto;
- administrar lixo;
- administrar transporte;
- controlar impostos;
- acompanhar população;
- acompanhar dinheiro;
- observar trânsito;
- acompanhar felicidade;
- investigar problemas.

A cidade deve parecer viva.

---

# 4. GRÁFICOS

O objetivo visual é alcançar uma qualidade semelhante à experiência visual de **Cities: Skylines**, sem copiar assets, identidade visual ou propriedade intelectual.

A referência visual deve ser:

- cidade 3D;
- câmera isométrica/estratégica;
- zoom livre;
- rotação;
- iluminação dinâmica;
- ciclo dia/noite;
- chuva;
- neblina;
- sombras;
- tráfego;
- milhares de objetos;
- cidadãos visíveis;
- veículos;
- prédios;
- árvores;
- parques;
- água;
- estradas;
- infraestrutura.

O visual deve priorizar:

> **"Uma cidade que parece viva."**

Não é necessário buscar fotorealismo absoluto.

O objetivo é atingir um visual:

- moderno;
- limpo;
- detalhado;
- estilizado-realista;
- agradável em câmera distante;
- legível em zoom próximo.

---

# 5. ENGINE PRINCIPAL

A tecnologia padrão proposta é:

## Unity 6

Justificar formalmente a escolha.

Avaliar também Unreal Engine, mas considerar que o projeto é especificamente focado em:

- C#;
- programação;
- simulação;
- ferramentas educacionais;
- execução de código C#;
- integração com compilador;
- arquitetura de software.

Portanto, Unity provavelmente deverá ser a escolha principal.

Criar:

`ENGINE_DECISION.md`

O documento deve comparar:

### Unity

- C# nativo;
- ECS;
- DOTS;
- Burst;
- Jobs;
- Entities Graphics;
- HDRP;
- AI Navigation;
- Splines;
- UI Toolkit;
- Terrain;
- ferramentas de editor;
- ecossistema.

### Unreal

- qualidade gráfica;
- C++;
- Blueprints;
- dificuldade para execução de C#;
- complexidade;
- integração externa;
- custo arquitetural para esse projeto.

A decisão deve ser técnica.

---

# 6. STACK TECNOLÓGICA RECOMENDADA

Avaliar e documentar os seguintes componentes.

## Engine

**Unity 6**

---

## Rendering

**HDRP**

Utilizar para:

- iluminação;
- sombras;
- volumetria;
- água;
- atmosfera;
- materiais;
- pós-processamento;
- ambientes urbanos.

---

## Simulation

Utilizar:

**Unity Entities / ECS**

para entidades em grande escala.

Exemplos:

- cidadãos;
- veículos;
- tráfego;
- agentes;
- partículas de simulação;
- consumo;
- produção;
- necessidades;
- sistemas repetitivos.

---

## Performance

Utilizar:

**Burst Compiler**

**C# Job System**

**Unity.Mathematics**

**Native Collections**

para processamento paralelo.

---

## Rendering em escala

Utilizar:

**Entities Graphics**

para:

- instancing;
- LOD;
- grandes quantidades de objetos;
- cidadãos;
- veículos;
- vegetação;
- elementos repetitivos.

---

## Navegação

Avaliar:

**AI Navigation / NavMesh**

para:

- pedestres;
- áreas navegáveis;
- obstáculos;
- conexões;
- escadas;
- passagens.

Porém:

> Não assumir que milhares de cidadãos devem ser implementados como milhares de `NavMeshAgent`.

Projetar uma solução escalável.

Possível arquitetura:

```text
City Navigation Graph
        ↓
Pathfinding
        ↓
Path Cache
        ↓
Agent Simulation
        ↓
Movement
        ↓
Rendering
```

Utilizar NavMesh onde fizer sentido, mas criar uma camada de abstração própria.

---

# 7. ESTRADAS

O sistema de estradas é um dos sistemas centrais.

O jogador deverá conseguir:

- criar rua;
- prolongar rua;
- conectar rua;
- criar cruzamento;
- remover rua;
- modificar largura;
- criar mão única;
- criar mão dupla;
- adicionar faixa;
- criar rotatória;
- criar ponte;
- criar túnel;
- criar semáforo;
- criar faixa de pedestre.

Avaliar **Unity Splines** para criação e edição de estradas curvas.

A estrada deve possuir dados como:

```text
Road
├── ID
├── Geometry
├── Lanes
├── SpeedLimit
├── Direction
├── TrafficCapacity
├── Connections
├── TrafficLights
└── Sidewalks
```

---

# 8. SISTEMA DE MAPA

O mapa deve ser um sistema real.

O jogador deverá conseguir:

- aproximar;
- afastar;
- rotacionar;
- mover câmera;
- selecionar área;
- visualizar bairros;
- visualizar ruas;
- visualizar terrenos;
- visualizar zonas;
- visualizar infraestrutura.

Criar sistema de **camadas de informação**.

Exemplos:

```text
Mapa normal

Mapa de população

Mapa de felicidade

Mapa de renda

Mapa de energia

Mapa de água

Mapa de esgoto

Mapa de poluição

Mapa de tráfego

Mapa de crime

Mapa de saúde

Mapa de educação

Mapa de empregos

Mapa de densidade

Mapa de valor do terreno
```

Cada camada deve alterar visualmente o mapa.

---

# 9. MAP OVERLAYS

Criar um sistema genérico:

```csharp
public interface IMapOverlay
{
    Color GetColor(Vector3 position);
}
```

Exemplos:

```text
TrafficOverlay
PopulationOverlay
PollutionOverlay
HappinessOverlay
EnergyOverlay
WaterOverlay
CrimeOverlay
EducationOverlay
```

O jogador deve conseguir alternar entre essas visualizações.

Esse sistema também deverá ser utilizado pedagogicamente para ensinar:

- interfaces;
- polimorfismo;
- abstração;
- Strategy Pattern.

---

# 10. TERRENO

Criar sistema de terreno.

Utilizar Unity Terrain quando apropriado.

Suportar:

- elevação;
- rios;
- lagos;
- praias;
- montanhas;
- estradas;
- vegetação;
- áreas construíveis;
- terrenos proibidos.

O terreno deve afetar a cidade.

Exemplo:

Construir em uma região muito inclinada pode:

- aumentar custo;
- dificultar estrada;
- aumentar risco;
- alterar valor imobiliário.

---

# 11. ZONEAMENTO

Criar sistema semelhante ao de city builders.

Tipos:

```text
Residential
Commercial
Industrial
Office
Public
Park
Infrastructure
```

O jogador poderá definir zonas.

As zonas deverão gerar demanda.

Exemplo:

```text
Alta demanda residencial
        ↓
Casas começam a surgir
        ↓
População aumenta
        ↓
Demanda comercial aumenta
        ↓
Comércios surgem
        ↓
Empregos aumentam
```

---

# 12. CIDADÃOS

Os cidadãos devem ser agentes reais.

Cada cidadão poderá possuir:

```text
Citizen
├── Identity
├── Age
├── Household
├── Home
├── Job
├── Income
├── Money
├── Health
├── Education
├── Happiness
├── Needs
├── Schedule
└── CurrentActivity
```

O cidadão deve:

- acordar;
- sair de casa;
- ir trabalhar;
- trabalhar;
- receber salário;
- comprar produtos;
- voltar para casa;
- utilizar serviços;
- descansar;
- consumir;
- eventualmente mudar de residência;
- eventualmente sair da cidade.

---

# 13. NECESSIDADES

Criar sistema de necessidades.

Exemplos:

```text
Hunger
Health
Entertainment
Safety
Education
Housing
Employment
Transport
Social
Environment
```

Essas necessidades devem influenciar felicidade.

---

# 14. FELICIDADE

Criar um sistema real de felicidade.

Exemplo:

```text
Happiness =
    Housing
  + Employment
  + Health
  + Safety
  + Education
  + Entertainment
  + Environment
  + Transport
  - Pollution
  - Traffic
  - Taxes
```

Não usar simplesmente uma variável arbitrária.

Criar um modelo de domínio documentado.

O jogador deverá conseguir investigar:

> "Por que meus cidadãos estão infelizes?"

O jogo deve mostrar causas.

---

# 15. TERMÔMETRO DA CIDADE

Criar um **City Temperature / City Health Indicator**.

Esse termômetro representa o estado geral da cidade.

Exemplo:

```text
🔥 CRÍTICO
🔴 Ruim
🟠 Instável
🟡 Atenção
🟢 Saudável
🔵 Excelente
```

O termômetro deve considerar:

- felicidade;
- desemprego;
- criminalidade;
- poluição;
- trânsito;
- saúde;
- educação;
- economia;
- energia;
- água;
- serviços.

Não deve ser apenas decorativo.

Criar:

`CITY_HEALTH_SYSTEM.md`

---

# 16. ECONOMIA

A cidade precisa possuir dinheiro circulando.

Não criar apenas:

```text
PlayerMoney += 100
```

Criar uma economia simplificada.

Fluxo:

```text
Empresa
    ↓
Produção
    ↓
Emprego
    ↓
Salário
    ↓
Cidadão
    ↓
Consumo
    ↓
Empresa
    ↓
Receita
    ↓
Impostos
    ↓
Governo
    ↓
Serviços públicos
    ↓
População
```

O dinheiro deve possuir origem e destino.

---

# 17. ORÇAMENTO DA CIDADE

A prefeitura deverá possuir:

```text
Revenue
├── Residential Tax
├── Commercial Tax
├── Industrial Tax
├── Service Fees
└── Other Revenue

Expenses
├── Education
├── Healthcare
├── Police
├── Fire Department
├── Garbage
├── Roads
├── Public Transport
├── Electricity
├── Water
└── Infrastructure
```

O jogador deverá tomar decisões.

Exemplo:

> A cidade está gastando mais do que arrecada.

O jogador poderá:

- aumentar impostos;
- reduzir serviços;
- construir infraestrutura mais eficiente;
- estimular economia;
- reduzir desperdícios.

---

# 18. SISTEMA DE MERCADO

Criar uma simulação simplificada de:

- oferta;
- demanda;
- preço;
- produção;
- consumo;
- empregos.

Exemplo:

Se houver muitas lojas e poucos consumidores:

```text
Oferta ↑
Demanda ↓
Lucro ↓
```

Algumas empresas podem fechar.

---

# 19. EMPRESAS

Criar empresas reais dentro da simulação.

Exemplos:

```text
Farm
Factory
Restaurant
Shop
Office
LogisticsCompany
ConstructionCompany
```

Cada empresa:

- possui funcionários;
- possui despesas;
- possui receitas;
- produz ou vende algo;
- paga salários;
- paga impostos;
- pode crescer;
- pode falir.

---

# 20. TRÂNSITO

Criar sistema de tráfego.

Veículos devem:

- possuir origem;
- possuir destino;
- escolher rota;
- seguir ruas;
- respeitar semáforos;
- parar;
- acelerar;
- mudar de faixa;
- sofrer congestionamento.

Criar:

```text
TrafficNetwork
RoadNetwork
Intersection
Lane
TrafficLight
Vehicle
Route
```

O trânsito deve afetar:

- tempo de deslocamento;
- felicidade;
- economia;
- poluição;
- produtividade.

---

# 21. TRANSPORTE PÚBLICO

Posteriormente:

- ônibus;
- metrô;
- trem;
- estações;
- linhas;
- rotas;
- passageiros.

O jogador deverá criar linhas.

O sistema deverá calcular:

- passageiros;
- tempo;
- cobertura;
- custo;
- eficiência.

---

# 22. ENERGIA

Criar:

```text
PowerPlant
PowerGrid
PowerLine
Consumer
```

A cidade deve possuir geração e consumo.

Exemplo:

```text
Geração: 100 MW
Consumo: 120 MW

Resultado:
Déficit de 20 MW
```

Isso deverá provocar consequências reais.

---

# 23. ÁGUA

Criar:

```text
WaterSource
WaterPump
WaterPipe
WaterTreatment
WaterConsumer
Sewage
```

Problemas reais:

- falta de água;
- baixa pressão;
- contaminação;
- capacidade insuficiente;
- vazamentos.

---

# 24. LIXO

Criar:

- produção de lixo;
- coleta;
- caminhões;
- aterros;
- reciclagem;
- capacidade;
- rotas.

Problemas:

> "O bairro norte está acumulando lixo."

O jogador deverá investigar.

---

# 25. SERVIÇOS PÚBLICOS

Implementar progressivamente:

- polícia;
- bombeiros;
- hospitais;
- escolas;
- universidades;
- parques;
- transporte;
- coleta de lixo.

Cada serviço deve possuir:

- custo;
- capacidade;
- cobertura;
- eficiência;
- funcionários;
- demanda.

---

# 26. SISTEMA DE PROBLEMAS

Esse sistema é fundamental.

A cidade deve gerar problemas naturalmente.

Exemplos:

```text
Trânsito intenso
Falta de energia
Falta de água
Lixo acumulado
Desemprego
Criminalidade
Poluição
Falta de moradia
Baixa educação
Baixa saúde
Déficit financeiro
Falta de transporte
```

O jogador deverá investigar.

---

# 27. PROBLEMAS COMO DESAFIOS DE PROGRAMAÇÃO

Um problema urbano deve poder se transformar em uma missão de programação.

Exemplo:

> O sistema de coleta de lixo está lento.

O jogo poderá pedir:

> "Crie um algoritmo que encontre o centro de coleta mais próximo."

O jogador aprende:

- loops;
- collections;
- métodos;
- algoritmos;
- interfaces;
- abstração.

Outro exemplo:

> "Diferentes veículos precisam calcular suas rotas."

Isso pode introduzir:

- herança;
- polimorfismo;
- interfaces;
- Strategy Pattern.

---

# 28. PROGRAMAÇÃO COMO FERRAMENTA DE GESTÃO

O jogador deverá escrever código para:

- construir;
- administrar;
- analisar;
- automatizar;
- resolver problemas.

Exemplo:

```csharp
public class GarbageCollector
{
    public void Collect(GarbageArea area)
    {
        // implementar
    }
}
```

O jogador executa.

A cidade reage.

---

# 29. API EDUCACIONAL

Não expor toda a engine.

Criar uma API própria.

Exemplo:

```csharp
city.Build(...)
city.GetPopulation()
city.GetCitizens()
city.GetRoads()
city.GetBudget()
city.GetHappiness()
city.GetTraffic()
```

O jogador deve aprender a programar contra uma API de domínio.

Nunca:

```csharp
GameObject.Find(...)
```

ou APIs internas indiscriminadas.

---

# 30. DOMÍNIO

Criar um domínio independente da Unity.

Exemplo:

```text
Domain
├── City
├── Citizen
├── Household
├── Building
├── Road
├── Vehicle
├── Company
├── Economy
├── Energy
├── Water
├── Education
├── Healthcare
├── Crime
├── Happiness
└── Transportation
```

O domínio não deve conhecer:

- GameObject;
- MonoBehaviour;
- Camera;
- Renderer;
- Transform;
- Unity UI.

---

# 31. ARQUITETURA HÍBRIDA

Utilizar arquitetura híbrida.

## ECS/DOTS

Para:

- agentes em massa;
- simulação;
- tráfego;
- cidadãos;
- processamento;
- entidades repetitivas.

## GameObjects

Para:

- ferramentas de edição;
- prédios especiais;
- UI;
- câmera;
- elementos complexos;
- objetos únicos;
- interação direta.

## Domain

Para:

- regras;
- economia;
- felicidade;
- necessidades;
- regras urbanas.

A arquitetura deve evitar transformar todo o projeto em ECS simplesmente porque ECS existe.

---

# 32. SEPARAÇÃO

Arquitetura conceitual:

```text
                ┌────────────────────┐
                │     Presentation   │
                │ Unity / HDRP / UI  │
                └─────────┬──────────┘
                          │
                ┌─────────▼──────────┐
                │     Application    │
                │ Commands / Queries │
                └─────────┬──────────┘
                          │
                ┌─────────▼──────────┐
                │       Domain       │
                │ City Simulation     │
                └─────────┬──────────┘
                          │
                ┌─────────▼──────────┐
                │   Infrastructure   │
                │ Persistence / Code │
                └────────────────────┘
```

---

# 33. SIMULAÇÃO EM TICK

A cidade não deve depender diretamente do `Update()` de cada objeto.

Criar uma simulação baseada em ticks.

Exemplo:

```text
Simulation Tick
       ↓
Economy
       ↓
Population
       ↓
Needs
       ↓
Traffic
       ↓
Energy
       ↓
Water
       ↓
Services
       ↓
Happiness
       ↓
Events
       ↓
Rendering
```

Definir frequência adequada para cada sistema.

Exemplo:

```text
Render: 60 FPS

Movement: 30 Hz

Traffic: 10-20 Hz

Citizen Needs: 1-5 Hz

Economy: 1 Hz

Population Growth: 1/Hour
```

Esses valores são exemplos iniciais e devem ser avaliados através de profiling.

---

# 34. SIMULAÇÃO ACELERADA

O jogador deverá controlar velocidade:

```text
Pause
1x
2x
3x
5x
10x
```

A simulação deve ser desacoplada do frame rate.

---

# 35. CÂMERA

Criar câmera semelhante a city builders.

Suportar:

- pan;
- zoom;
- rotação;
- inclinação;
- foco;
- seleção;
- seguir cidadão;
- seguir veículo;
- visão geral;
- visão de bairro.

---

# 36. VISUALIZAÇÃO DE DADOS

Criar UI semelhante a ferramentas de análise.

Ao clicar em uma casa:

```text
Casa #1029

Moradores: 4

Consumo de energia: 3.2 MW
Consumo de água: 1.4 m³

Felicidade:
████████░░ 82%

Valor:
$120.000

Problemas:
Nenhum
```

Ao clicar em cidadão:

```text
João Silva

Idade: 31
Profissão: Engenheiro
Salário: $4.500

Casa:
Bairro Central

Trabalho:
Tech Company

Felicidade:
78%

Próxima atividade:
Trabalhar

Destino:
Tech Company

Rota:
Casa → Rua A → Avenida B → Empresa
```

---

# 37. MODO DE INVESTIGAÇÃO

Criar uma ferramenta semelhante a um "Inspector".

O jogador poderá clicar em:

- cidadão;
- prédio;
- rua;
- veículo;
- empresa;
- estação;
- rede elétrica;
- tubulação.

E visualizar:

- estado;
- propriedades;
- relações;
- dependências;
- histórico;
- problemas.

Isso também deve servir para ensinar debugging.

---

# 38. DEBUG VISUAL

Criar modos:

```text
Show Citizen Paths

Show Traffic Routes

Show Electricity Network

Show Water Network

Show Garbage Routes

Show Job Connections

Show Household Connections

Show Supply Chain

Show Money Flow
```

Exemplo:

Ao selecionar um cidadão:

```text
Casa
 ↓
Trabalho
 ↓
Restaurante
 ↓
Casa
```

Mostrar visualmente as conexões.

---

# 39. MONEY FLOW VISUALIZATION

Criar modo:

> "Fluxo de dinheiro"

Visualizar:

```text
Empresa
   │
   │ salário
   ▼
Cidadão
   │
   │ consumo
   ▼
Empresa
   │
   │ imposto
   ▼
Prefeitura
   │
   │ serviço
   ▼
Cidade
```

Isso transforma economia em algo visual.

---

# 40. SISTEMA DE EVENTOS

Criar eventos dinâmicos.

Exemplos:

- enchente;
- incêndio;
- crise econômica;
- epidemia;
- greve;
- acidente;
- congestionamento;
- apagão;
- seca;
- crescimento populacional.

Os eventos devem alterar a simulação.

---

# 41. PROGRAMAÇÃO ORIENTADA A OBJETOS

O conteúdo educacional deverá ser integrado ao gameplay.

## Classe

O jogador cria:

```csharp
public class Hospital
{
}
```

E um hospital aparece no jogo.

## Objeto

```csharp
var hospital = new Hospital();
```

## Propriedade

```csharp
hospital.Capacidade = 100;
```

## Método

```csharp
hospital.AtenderPaciente();
```

## Encapsulamento

O jogador precisa proteger estado interno.

## Composição

```text
Hospital
 ├── EmergencyDepartment
 ├── Pharmacy
 └── Staff
```

## Herança

```text
Building
 ├── ResidentialBuilding
 ├── CommercialBuilding
 └── IndustrialBuilding
```

## Polimorfismo

```csharp
building.CalculateMaintenanceCost();
```

Cada prédio implementa sua regra.

---

# 42. SOLID

Ensinar SOLID através de problemas reais.

Exemplo:

A classe:

```text
CityManager
```

começa fazendo tudo.

O jogo deve permitir que o jogador perceba:

> "Essa classe ficou impossível de manter."

Então introduzir:

```text
City
EconomyService
TrafficService
CitizenService
EnergyService
WaterService
```

Ensinar:

- SRP;
- OCP;
- LSP;
- ISP;
- DIP.

---

# 43. DESIGN PATTERNS

Introduzir somente quando resolver um problema real.

Possíveis padrões:

```text
Factory
Strategy
Observer
Command
State
Builder
Specification
Repository
Mediator
```

Cada padrão deve nascer de uma necessidade da cidade.

---

# 44. TESTES

O jogador também deverá aprender testes.

Exemplo:

```csharp
[Test]
public void ShouldCalculateElectricityConsumption()
{
}
```

Criar desafios onde o jogador precisa:

- criar teste;
- corrigir implementação;
- detectar bug;
- refatorar.

---

# 45. CODE EDITOR

Utilizar um editor integrado.

Avaliar:

- Monaco Editor;
- Roslyn;
- syntax highlighting;
- autocomplete;
- diagnostics;
- IntelliSense;
- code completion;
- documentação contextual.

Criar:

`CODE_EDITOR.md`

---

# 46. COMPILAÇÃO

Utilizar Roslyn onde apropriado para:

- parsing;
- compilation;
- diagnostics;
- análise do código;
- validação.

O sistema deverá separar:

```text
Editor
   ↓
Parser
   ↓
Analyzer
   ↓
Validator
   ↓
Compiler
   ↓
Sandbox
   ↓
Execution
   ↓
Simulation
```

---

# 47. SEGURANÇA DO CÓDIGO

Esse é um dos sistemas mais críticos.

Nunca permitir que código do jogador tenha acesso irrestrito ao computador.

Bloquear ou controlar:

- filesystem;
- processos;
- rede;
- reflection perigosa;
- carregamento arbitrário de assemblies;
- APIs não autorizadas;
- acesso ao sistema operacional;
- execução externa.

Criar:

`CODE_EXECUTION_SECURITY.md`

Pesquisar soluções modernas de sandboxing e isolamento.

Não assumir que:

```csharp
AssemblyLoadContext
```

sozinho fornece sandbox de segurança.

---

# 48. VALIDAÇÃO DE CÓDIGO

Não validar somente texto.

Validar:

### Syntax

Código válido.

### Compilation

Código compila.

### Runtime

Código executa.

### Behavior

Código produz comportamento correto.

### Architecture

Em desafios avançados, verificar também:

- uso correto de interface;
- separação de responsabilidade;
- ausência de dependências proibidas;
- utilização do padrão ensinado.

---

# 49. SISTEMA DE DICAS

Cada desafio deve possuir:

```text
Hint 1
Hint 2
Hint 3
Partial Solution
Complete Solution
Explanation
```

O jogo deve evitar entregar a solução imediatamente.

---

# 50. SISTEMA DE APRENDIZADO

Criar árvore:

```text
C# Fundamentals
        ↓
Methods
        ↓
Classes
        ↓
Objects
        ↓
Encapsulation
        ↓
Composition
        ↓
Inheritance
        ↓
Polymorphism
        ↓
Interfaces
        ↓
SOLID
        ↓
Patterns
        ↓
Architecture
```

Cada conceito deve possuir:

- teoria;
- exemplo;
- desafio;
- aplicação na cidade;
- erro;
- feedback;
- exercício;
- revisão.

---

# 51. SISTEMA DE PROBLEMAS PEDAGÓGICOS

Quando o jogador errar:

Não dizer apenas:

> Compilation Error.

Mostrar:

```text
Problema na cidade:

A estação de energia não está funcionando.

Problema detectado:

O método CalculateEnergyConsumption()
retornou um valor incorreto.

Conceito relacionado:

Métodos e retorno.

Dica:

O método precisa informar quanto de energia
está sendo consumido.
```

---

# 52. SISTEMA DE REPLAY

O jogador deverá poder:

- voltar;
- testar código;
- comparar resultados;
- restaurar estado;
- executar novamente.

Para desafios, considerar snapshots da simulação.

---

# 53. PERSISTÊNCIA

Salvar:

```text
City
PlayerProgress
PlayerCode
Challenges
Buildings
Citizens
Economy
SimulationState
Achievements
Settings
```

Projetar sistema de save versionado.

---

# 54. PERFORMANCE

Definir orçamento de performance.

O jogo deve ser projetado para suportar progressivamente:

```text
1.000 cidadãos
10.000 cidadãos
50.000 cidadãos
100.000+ cidadãos
```

Não assumir que todos os agentes precisam de simulação completa individualmente.

Utilizar níveis de simulação.

Exemplo:

```text
Citizen próximo
→ simulação detalhada

Citizen distante
→ simulação simplificada

Citizen muito distante
→ estatística agregada
```

Criar:

`SIMULATION_LOD.md`

---

# 55. LEVEL OF DETAIL DA SIMULAÇÃO

Assim como existe LOD gráfico, criar LOD de simulação.

### Level 0

Cidadão próximo:

- rota;
- animação;
- necessidades;
- comportamento detalhado.

### Level 1

Cidadão distante:

- posição;
- destino;
- estado simplificado.

### Level 2

Cidadão muito distante:

- somente estatísticas agregadas.

---

# 56. ASSET MANAGEMENT

Avaliar:

- Addressables;
- LOD;
- GPU instancing;
- Entities Graphics;
- streaming;
- asset bundles quando apropriado.

Criar:

`ASSET_PIPELINE.md`

---

# 57. VEGETAÇÃO

Criar sistema para:

- árvores;
- grama;
- parques;
- vegetação urbana.

Evitar milhares de GameObjects individuais quando uma abordagem instanciada for mais adequada.

---

# 58. ÁGUA

Avaliar recursos do HDRP e soluções complementares para:

- rios;
- lagos;
- mar;
- reflexos;
- transparência;
- ondas;
- água urbana.

---

# 59. CLIMA

Criar:

- chuva;
- sol;
- nuvens;
- neblina;
- tempestades.

Clima deverá afetar:

- trânsito;
- felicidade;
- consumo;
- energia;
- acidentes;
- produtividade.

---

# 60. CICLO DIA/NOITE

Criar:

```text
Morning
Day
Evening
Night
```

Isso deve afetar:

- iluminação;
- tráfego;
- comércio;
- trabalho;
- cidadãos;
- energia;
- criminalidade.

---

# 61. UI

Utilizar **UI Toolkit** para interfaces de gerenciamento quando fizer sentido.

Criar:

- painéis;
- menus;
- gráficos;
- inspector;
- overlays;
- árvore de conhecimento;
- editor;
- console;
- estatísticas.

A UI deve ter aparência de software profissional.

---

# 62. CONSOLE DO JOGADOR

Criar console semelhante a uma IDE.

Mostrar:

```text
Build succeeded.

3 warnings.

Challenge passed.

Population: 2,340
Happiness: 78%
Treasury: $124,000
```

Erros devem ser clicáveis e levar ao código correspondente.

---

# 63. ARQUITETURA DE PASTAS

Criar estrutura semelhante a:

```text
Assets/
    Art/
    Audio/
    Materials/
    Prefabs/

    Code/
        Domain/
        Application/
        Infrastructure/
        Presentation/

    ECS/
        Citizens/
        Vehicles/
        Traffic/
        Simulation/

    Education/
        Challenges/
        Lessons/
        Hints/
        Validators/

    CodeExecution/
        Compiler/
        Analyzer/
        Sandbox/

    City/
        Buildings/
        Roads/
        Terrain/
        Economy/
        Utilities/
        Services/

    UI/
        Editor/
        City/
        Inspector/
        Overlays/
```

Adaptar conforme decisões arquiteturais.

---

# 64. FRAMEWORKS E TECNOLOGIAS A AVALIAR

Criar uma seção específica documentando cada tecnologia.

No mínimo avaliar:

### Unity

Engine.

### HDRP

Rendering.

### Entities / ECS

Simulação em escala.

### Burst

Performance.

### C# Job System

Paralelismo.

### Unity.Mathematics

Matemática eficiente.

### Entities Graphics

Rendering em escala.

### AI Navigation

Navegação.

### Splines

Estradas e curvas.

### UI Toolkit

Interface.

### Addressables

Gerenciamento de assets.

### Roslyn

Compilação/análise C#.

### Monaco Editor

Editor de código.

### NUnit / Unity Test Framework

Testes.

### Profiler

Performance.

### Git

Versionamento.

### Git LFS

Assets grandes.

---

# 65. TECNOLOGIAS QUE NÃO DEVEM SER ADICIONADAS SEM JUSTIFICATIVA

Não adicionar frameworks apenas porque são populares.

Toda dependência deverá responder:

1. Qual problema resolve?
2. É realmente necessária?
3. Podemos resolver com Unity?
4. Qual impacto de manutenção?
5. Qual impacto de performance?
6. Qual risco de dependência?

Criar ADR quando a decisão for arquiteturalmente relevante.

---

# 66. DOCUMENTAÇÃO

Criar:

```text
/docs

README.md

VISION.md

GAME_DESIGN.md

GAMEPLAY.md

LEARNING_DESIGN.md

LEARNING_PATH.md

ENGINE_DECISION.md

TECH_STACK.md

ARCHITECTURE.md

DOMAIN_MODEL.md

SIMULATION_ARCHITECTURE.md

SIMULATION_TICK.md

SIMULATION_LOD.md

CITY_SIMULATION.md

CITIZEN_SYSTEM.md

HOUSEHOLD_SYSTEM.md

BUILDING_SYSTEM.md

ROAD_SYSTEM.md

TRAFFIC_SYSTEM.md

VEHICLE_SYSTEM.md

PATHFINDING.md

PUBLIC_TRANSPORT.md

ZONE_SYSTEM.md

ECONOMY_SYSTEM.md

MONEY_FLOW.md

BUDGET_SYSTEM.md

HAPPINESS_SYSTEM.md

CITY_HEALTH_SYSTEM.md

ENERGY_SYSTEM.md

WATER_SYSTEM.md

SEWAGE_SYSTEM.md

GARBAGE_SYSTEM.md

POLLUTION_SYSTEM.md

EDUCATION_SYSTEM.md

HEALTHCARE_SYSTEM.md

CRIME_SYSTEM.md

EVENT_SYSTEM.md

WEATHER_SYSTEM.md

DAY_NIGHT_SYSTEM.md

MAP_SYSTEM.md

MAP_OVERLAYS.md

DEBUG_VISUALIZATION.md

CAMERA_SYSTEM.md

CODE_EDITOR.md

CODE_EXECUTION.md

CODE_EXECUTION_SECURITY.md

CODE_ANALYSIS.md

VALIDATION_SYSTEM.md

CHALLENGE_SYSTEM.md

HINT_SYSTEM.md

ERROR_SYSTEM.md

LEARNING_ENGINE.md

PROGRESSION.md

SOLID_LEARNING.md

DESIGN_PATTERNS.md

TESTING.md

SAVE_SYSTEM.md

ASSET_PIPELINE.md

GRAPHICS.md

LIGHTING.md

AUDIO.md

UI_UX.md

PERFORMANCE.md

PROFILING.md

SECURITY.md

CODING_STANDARDS.md

AI_DEVELOPMENT_GUIDE.md

DEVELOPMENT_ROADMAP.md

MVP.md

RISKS.md

FUTURE_FEATURES.md

ADR/
```

---

# 67. ADRs

Criar:

```text
ADR/
    0001-engine-selection.md
    0002-render-pipeline.md
    0003-ecs-strategy.md
    0004-domain-architecture.md
    0005-code-execution.md
    0006-code-security.md
    0007-code-editor.md
    0008-simulation-tick.md
    0009-citizen-simulation.md
    0010-pathfinding.md
    0011-road-system.md
    0012-persistence.md
    0013-map-overlay-system.md
```

---

# 68. MVP

O MVP NÃO deve possuir 100 mil cidadãos.

Criar uma fatia vertical completa.

O primeiro MVP deve ter:

### Mundo

- terreno;
- câmera;
- iluminação;
- pequena cidade.

### Construção

- estrada;
- casa;
- comércio;
- prefeitura.

### Simulação

- população;
- dinheiro;
- felicidade;
- energia.

### Código

Editor C#.

### Aprendizado

Ensinar:

- variável;
- método;
- classe;
- objeto;
- propriedade.

### Integração

O jogador escreve:

```csharp
var house = new House();
city.Build(house);
```

A casa realmente aparece.

Depois:

```csharp
house.EnergyConsumption = 5;
```

A simulação realmente muda.

### Feedback

Se estiver errado:

- erro;
- explicação;
- dica.

---

# 69. PRIMEIRO VERTICAL SLICE

Antes de construir o jogo inteiro, implementar apenas:

```text
Terreno
   ↓
Estrada
   ↓
Casa
   ↓
Cidadão
   ↓
Dinheiro
   ↓
Energia
   ↓
Felicidade
   ↓
Código C#
   ↓
Validação
   ↓
Feedback
```

Se essa experiência funcionar, o restante do jogo poderá crescer sobre ela.

---

# 70. ROADMAP

Criar roadmap semelhante a:

## Fase 0

Pesquisa e arquitetura.

## Fase 1

Protótipo gráfico.

## Fase 2

Sistema de mapa e câmera.

## Fase 3

Estradas.

## Fase 4

Construções.

## Fase 5

Primeiros cidadãos.

## Fase 6

Economia.

## Fase 7

Energia e água.

## Fase 8

Trânsito.

## Fase 9

Felicidade e necessidades.

## Fase 10

Problemas urbanos.

## Fase 11

Editor C#.

## Fase 12

Execução segura.

## Fase 13

Desafios.

## Fase 14

POO.

## Fase 15

SOLID.

## Fase 16

Design Patterns.

## Fase 17

Testes.

## Fase 18

Otimização ECS/DOTS.

## Fase 19

Conteúdo.

## Fase 20

Polimento.

---

# 71. PRINCÍPIO DE DESIGN

O jogo deve seguir:

> "Problema primeiro, teoria depois."

Não ensinar:

> "Hoje vamos aprender Strategy Pattern."

Em vez disso:

> "Sua cidade possui 12 tipos diferentes de transporte. Seu código está cheio de ifs."

O jogador tenta resolver.

Depois o jogo apresenta:

> "Existe uma forma de separar cada estratégia de transporte."

Então introduzir Strategy.

---

# 72. PRINCÍPIO DE POO

A cidade deve naturalmente produzir problemas de engenharia.

O jogador deve perceber:

> "Essa classe está ficando grande."

> "Estou repetindo código."

> "Preciso de uma interface."

> "Preciso separar essa responsabilidade."

> "Preciso de polimorfismo."

> "Preciso de uma abstração."

Esse é o aprendizado.

---

# 73. O JOGO DEVE TER DUAS CAMADAS

## Camada 1 — City Builder

O jogador:

- constrói;
- administra;
- expande;
- resolve problemas;
- observa cidadãos.

## Camada 2 — Software Engineering

O jogador:

- escreve código;
- modela objetos;
- cria classes;
- refatora;
- testa;
- depura;
- cria abstrações.

As duas camadas devem estar completamente conectadas.

---

# 74. EXEMPLO DE MISSÃO COMPLETA

Missão:

> "A cidade precisa de um sistema de coleta de lixo."

O jogador recebe:

```text
Problema:

O bairro norte está acumulando lixo.

Objetivo:

Criar um sistema capaz de identificar áreas
que precisam de coleta.
```

O jogador recebe:

```csharp
public class GarbageCollector
{
    public void Collect(...)
    {
        // seu código
    }
}
```

Ele aprende:

- classe;
- método;
- parâmetros;
- collections;
- loops.

Depois:

> "Existem diferentes tipos de veículos."

Introduzir:

```text
GarbageVehicle
Bus
PoliceCar
FireTruck
```

E posteriormente:

```text
Vehicle
   ↓
GarbageVehicle
Bus
PoliceCar
FireTruck
```

Introduzir herança.

Depois:

> "Agora todos precisam possuir uma maneira diferente de calcular rota."

Introduzir interface.

Depois:

> "O sistema ficou difícil de manter."

Introduzir Strategy/SOLID.

Isso deve acontecer organicamente.

---

# 75. NÃO IMPLEMENTAR TUDO DE UMA VEZ

A IA responsável pelo desenvolvimento deve impedir expansão descontrolada.

Sempre perguntar:

> "Essa funcionalidade pertence ao MVP atual?"

Se não:

- documentar;
- colocar no roadmap;
- não implementar.

---

# 76. DOCUMENTAÇÃO PARA OUTRAS IAS

Criar:

`AI_DEVELOPMENT_GUIDE.md`

Toda IA que trabalhar no projeto deverá:

1. Ler `README.md`.
2. Ler `ARCHITECTURE.md`.
3. Ler `TECH_STACK.md`.
4. Ler o documento do sistema que será alterado.
5. Ler ADRs relacionados.
6. Inspecionar o código existente.
7. Identificar dependências.
8. Propor alteração.
9. Implementar.
10. Criar testes.
11. Executar testes.
12. Atualizar documentação.
13. Verificar regressões.

A IA nunca deve:

- criar arquitetura paralela;
- duplicar sistemas;
- adicionar dependências sem justificativa;
- colocar lógica de domínio em MonoBehaviour;
- ignorar ECS quando a escala exigir;
- transformar tudo em ECS sem necessidade;
- criar Singleton global sem ADR;
- adicionar abstrações sem necessidade.

---

# 77. CRITÉRIO DE QUALIDADE

A documentação deve ser suficientemente detalhada para que outra IA consiga começar o desenvolvimento sem precisar perguntar:

> "O que vocês estavam pensando?"

Os documentos devem explicar:

- intenção;
- arquitetura;
- responsabilidades;
- contratos;
- dependências;
- tecnologias;
- limitações;
- performance;
- segurança;
- decisões;
- exemplos;
- testes.

---

# 78. ANÁLISE CRÍTICA OBRIGATÓRIA

Antes de criar a documentação, faça uma análise crítica do projeto.

Identifique:

- funcionalidades excessivamente complexas;
- gargalos;
- riscos de performance;
- riscos de segurança;
- problemas de execução de C#;
- riscos de simulação;
- riscos de pathfinding;
- riscos de memória;
- riscos de rendering;
- riscos de arquitetura;
- problemas pedagógicos.

Não concorde automaticamente comigo.

Se uma ideia for ruim, explique.

Se uma tecnologia não for adequada, proponha outra.

---

# 79. ENTREGÁVEL FINAL

Ao finalizar esta etapa, quero:

```text
/docs
```

com toda a documentação.

Também quero:

```text
PROJECT_ARCHITECTURE.md
```

com um diagrama geral.

E:

```text
TECH_STACK.md
```

com a stack completa.

E:

```text
MVP.md
```

com o escopo exato da primeira versão.

E:

```text
DEVELOPMENT_ROADMAP.md
```

com todas as fases.

---

# 80. PRINCÍPIO CENTRAL DO PROJETO

Toda decisão deve ser avaliada com base nesta frase:

> **"O jogador deve sentir que está aprendendo engenharia de software porque está tentando fazer uma cidade funcionar."**

Não quero um tutorial de C# com uma cidade decorativa.

Quero uma **cidade realmente simulada**, onde:

```text
Código
 ↓
Objetos
 ↓
Sistemas
 ↓
Simulação
 ↓
Cidade
 ↓
Cidadãos
 ↓
Problemas
 ↓
Feedback
 ↓
Aprendizado
```

O jogador deve poder olhar para sua cidade e pensar:

> "Essa cidade funciona porque eu programei os sistemas que fazem ela funcionar."

Esse é o produto.

Uma decisão que considero especialmente importante
Eu não faria a simulação inteira usando POO tradicional/GameObjects. Isso iria contra o próprio objetivo de ter uma cidade grande.
A divisão que eu colocaria como requisito no prompt é:
Sistema	Tecnologia
Renderização visual	Unity HDRP
Prédios especiais	GameObjects
Cidadãos em massa	ECS/DOTS
Veículos	ECS/DOTS
Tráfego	ECS + Jobs/Burst
Simulação econômica	Domain C#
Regras de negócio	Domain C#
Estradas	Splines + sistema próprio
Navegação	AI Navigation + sistema próprio de pathfinding
Editor C#	Monaco + integração própria
Compilação	Roslyn
Validação	Roslyn + testes + runtime
UI	UI Toolkit
Assets	Addressables + LOD
Performance	Burst + Jobs + Profiler
Testes	Unity Test Framework/NUnit
Versionamento	Git + Git LFS


Isso inclusive cria uma situação muito interessante pedagogicamente: o jogador aprende POO e, mais adiante, começa a perceber que nem todo problema deve ser resolvido com uma classe por objeto. Ele pode chegar a conceitos de composição, arquitetura, eventos, processamento em lote e até data-oriented design. O próprio ECS da Unity é projetado para escalar simulações e pode coexistir com GameObjects. Unity
E para os pedestres, eu faria uma distinção importante: pedestre visível ≠ cidadão totalmente simulado o tempo todo. Um cidadão próximo pode ter rotina, necessidades, caminho e animação detalhados; milhares de cidadãos distantes podem ser simulados de forma agregada. Isso é essencial para chegar a uma cidade realmente grande sem tentar calcular tudo no mesmo nível a cada frame.
A parte gráfica também pode ficar bastante próxima da sensação de um city builder moderno sem exigir que você faça um jogo AAA inteiro: HDRP + LOD + instancing/Entities Graphics + iluminação bem projetada + bons assets + câmera estratégica. O HDRP da Unity é direcionado a gráficos de alta fidelidade em PC/consoles, e o Entities Graphics oferece instancing e LOD para grandes quantidades de entidades. Unity
Eu começaria o projeto pelo vertical slice, não pelo sistema completo: terreno → rua → casa → 10 cidadãos → dinheiro → energia → felicidade → editor C# → código cria/altera uma casa → cidade reage. Se essa pequena versão for divertida e tecnicamente funcionar, aí sim vale escalar para trânsito, 10 mil cidadãos, economia, transporte público etc.