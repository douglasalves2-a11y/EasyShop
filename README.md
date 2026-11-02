
# 🛒 EasyShop

**Sistema de gerenciamento de clientes, produtos e vendas desenvolvido com .NET MAUI, utilizando arquitetura de banco de dados poliglota (SQL e NoSQL).**

## 📌 Sobre o projeto

O EasyShop é um aplicativo de gerenciamento comercial desenvolvido como projeto acadêmico, com o objetivo de aplicar conceitos de desenvolvimento mobile, persistência de dados e integração entre diferentes tecnologias de bancos de dados.

O diferencial do projeto é a utilização de uma **arquitetura poliglota**, na qual cada banco de dados é utilizado conforme as necessidades de armazenamento de cada módulo.

## 🚀 Tecnologias utilizadas

* **C#** — Linguagem de programação.
* **.NET MAUI** — Desenvolvimento da aplicação multiplataforma.
* **XAML** — Construção das interfaces.
* **SQLite** — Banco de dados relacional (SQL).
* **LiteDB** — Banco de dados não relacional (NoSQL).
* **Visual Studio** — Ambiente de desenvolvimento.
* **Android Studio** — Emulador para testes Android.

## 🏗️ Arquitetura Poliglota

O EasyShop utiliza dois bancos de dados diferentes:

| Banco de dados | Tecnologia | Responsabilidade                    |
| -------------- | ---------- | ----------------------------------- |
| Relacional     | SQLite     | Clientes e vendas                   |
| Não relacional | LiteDB     | Produtos e movimentações de estoque |

### SQLite — Banco relacional

Responsável pelo armazenamento de:

* Cadastro de clientes.
* Registro de vendas.
* Itens relacionados às vendas.
* Histórico de vendas.
* Status das vendas.

### LiteDB — Banco NoSQL

Responsável pelo armazenamento de:

* Cadastro de produtos.
* Descrição e categoria dos produtos.
* Preços e quantidades em estoque.
* Tags dos produtos.
* Movimentações de estoque relacionadas às vendas.

### Integração entre os bancos

Quando uma venda é realizada:

1. O sistema valida o cliente e os produtos selecionados.
2. Verifica a disponibilidade do estoque.
3. Registra a venda e seus itens no SQLite com status pendente.
4. Realiza a baixa do estoque no LiteDB.
5. Registra a movimentação para impedir descontos duplicados.
6. Atualiza o status da venda no SQLite para concluída.

O sistema também possui um mecanismo de recuperação de vendas pendentes, permitindo novas tentativas de processamento após interrupções.

Essa estratégia ajuda a manter a consistência dos dados entre os dois bancos, embora não represente uma transação única entre SQLite e LiteDB.

## ✨ Funcionalidades

### 👥 Clientes

* Cadastro de clientes.
* Listagem de clientes.
* Edição de informações.
* Exclusão de registros.
* Validação de dados.
* Verificação de e-mails duplicados.
* Persistência utilizando SQLite.

### 📦 Produtos

* Cadastro de produtos.
* Listagem de produtos.
* Edição de informações.
* Exclusão de produtos.
* Controle de preços.
* Controle de estoque.
* Categorias e tags.
* Persistência utilizando LiteDB.

### 💰 Vendas

* Seleção de clientes.
* Seleção de produtos.
* Definição de quantidades.
* Adição de vários produtos à venda.
* Cálculo automático dos subtotais.
* Cálculo automático do valor total.
* Registro do histórico de vendas.
* Consulta dos itens de cada venda.
* Persistência utilizando SQLite.

### 📊 Controle de estoque

* Baixa automática após a venda.
* Validação de estoque disponível.
* Bloqueio de vendas com estoque insuficiente.
* Agrupamento de produtos repetidos na mesma venda.
* Registro das movimentações.
* Proteção contra baixa duplicada.
* Recuperação de vendas pendentes.

## 📱 Telas do aplicativo

O EasyShop possui três módulos principais:

| Módulo   | Descrição                            |
| -------- | ------------------------------------ |
| Clientes | Gerenciamento de clientes            |
| Produtos | Gerenciamento de produtos e estoque  |
| Vendas   | Registro e acompanhamento das vendas |

> Capturas de tela da aplicação poderão ser adicionadas nesta seção.

## ⚙️ Requisitos para execução

Para executar o projeto, é necessário ter:

* Windows 10 ou superior.
* Visual Studio com suporte ao .NET MAUI.
* .NET SDK 10.
* Android SDK.
* Android Emulator ou dispositivo Android físico.

## ▶️ Como executar

### 1. Clonar o repositório

```bash
git clone URL_DO_REPOSITORIO
```

### 2. Acessar a pasta do projeto

```bash
cd EasyShop
```

### 3. Restaurar as dependências

```bash
dotnet restore
```

### 4. Compilar para Android

```bash
dotnet build -f net10.0-android
```

### 5. Executar no emulador

Com um emulador Android iniciado:

```bash
dotnet build -t:Run -f net10.0-android
```

Também é possível abrir a solução diretamente no Visual Studio e executar o projeto pelo ambiente de desenvolvimento.

## 🧪 Testes realizados

Foram realizados testes funcionais dos principais módulos:

* [x] Cadastro e consulta de clientes.
* [x] Edição e exclusão de clientes.
* [x] Persistência de clientes no SQLite.
* [x] Cadastro e consulta de produtos.
* [x] Edição e exclusão de produtos.
* [x] Persistência de produtos no LiteDB.
* [x] Registro de vendas.
* [x] Consulta do histórico de vendas.
* [x] Consulta dos itens de uma venda.
* [x] Baixa automática de estoque.
* [x] Bloqueio de venda com estoque insuficiente.
* [x] Persistência do estoque após reinicialização do aplicativo.

## 🎯 Objetivo acadêmico

O projeto tem como objetivo demonstrar, na prática, a aplicação de uma arquitetura de banco de dados poliglota em um aplicativo mobile.

A utilização conjunta de SQLite e LiteDB permite explorar as diferenças entre bancos relacionais e não relacionais, demonstrando como tecnologias distintas podem trabalhar em conjunto dentro de uma mesma aplicação.

## 👨‍💻 Desenvolvedores

👨‍💻 Douglas Henrique Alves
👨‍💻 Miller Carlos

Projeto acadêmico desenvolvido para aplicação prática de conceitos de:

* Desenvolvimento de aplicações mobile.
* Programação orientada a objetos.
* Arquitetura de software.
* Bancos de dados relacionais e não relacionais.
* Persistência e integração de dados.

---

**EasyShop — Gerenciamento comercial com arquitetura SQL + NoSQL.**
