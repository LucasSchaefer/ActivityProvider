# Activity Provider: Tradução Colaborativa

Tradução Colaborativa é um módulo Activity Provider voltado para a arquitetura Inven!RA, que permite a tradução colaborativa de documentos e textos, e também o peer review das traduções, sejam eles traduzidos de forma comercial ou com finalidade acadêmica. O objetivo deste módulo é permitir com que plataformas de tradução possam implementar a arquitetura Inven!RA e abstrair parte da gestão de correções de tradução e a tarefa de gerir os analytics relacionados às mesmas.
Pequenos trechos de texto ou frases serão atribuídos aos tradutores, que deverão traduzi-los da forma mais natural possível. A aplicação fornecerá ao tradutor uma visualização do texto original (para contexto) com o trecho a traduzir em destaque, e também um editor de texto onde o tradutor deverá inserir o texto traduzido. O revisor terá acesso à mesma interface, onde poderá corrigir as traduções e indicar os acertos e erros.

Autor: Lucas Schaefer

Deployment na plataforma Render: https://translator-activity-provider.onrender.com/


## Endpoints REST
### Página de configuração da atividade (config_url)
GET /config-translate
### Lista de parâmetros que se podem obter nessa página de configuração (json_params_url)
GET /json-params-translate
### URL de deploy da atividade (user_url)
GET /deploy-translate?activityID={id}
### Analytics de um utilizador da atividade (analytics_url)
POST /analytics-translate
### Lista de analytics disponíveis na atividade (analytics_list_url)
GET /analytics-list-translate
### Inicializa o processo para o Actor (user autenticado)
POST /process
### Altera o texto do processo para o Actor (user autenticado)
PATCH /process
### Obtém o status do processo para o Actor (user autenticado)
GET /status
### Completa o processo para o Actor (user autenticado)
GET /complete

## Padrão de Criação: FACTORY

Foi implementado um padrão Factory para disponibilizar interfaces de criação para objetos específicos para cada user.
Clientes, Tradutores e Revisores podem ter comportamentos diferentes, e com base no user autenticado se constrói um "Processo" especifico para aquele perfil.
As classes Processo específicas também contém uma implementação específica para cada perfil, e desacopla os endpoints de conhecerem cada tipo de processo.
Desta forma, tanto o endpoint AccessProcess (inicia/obtém o processo para o user) quanto os endpoints que performam alterações no processo estão desacoplados da implementação específica do perfil de ator.

/Factory/ActorProcessFactory.cs - Factory para os processos

/Models/Atores/ActorProcess.cs - classe base para o Processo

## Padrão de Estrutura: PROXY

Foi implementado um padrão Proxy, com o objetivo de atuar como um intermediário de proteção (Protection Proxy) entre o cliente e o repositório real de dados.
O serviço real nunca é chamado diretamente, mas sim o Proxy que partilha da mesma assinatura. 
Dessa forma, é possível adicionar uma camada extra de validação, sem expor diretamente o contrato do serviço que afeta a base de dados.

**O que ele faz antes de commitar na base de dados:**
1. **Validação**: Verifica regras de negócio (texto válido, valida status do processo)
2. **Auditoria**: Adiciona automaticamente timestamps e ID do usuário que modificou
3. **Normalização**: Sanitiza texto

**Fluxo completo:**
Cliente -> Proxy (valida/processa) -> Repositório Real -> Base de Dados

**Vantagens:**
- Centraliza toda lógica de validação e auditoria em um lugar
- Cliente não precisa saber das regras de negócio
- Fácil de testar e estender (novas validações só aqui)
- Transparente via DI - trocamos implementação sem quebrar código

**Exemplo**: Quando você chama `ChangeText()`, este proxy intercepta, processa o objeto Process e SÓ ENTÃO chama a base de dados real.

/Services/Proxy/ProcessProxyService.cs - Serviço Proxy que partilha assinatura com o serviço "real"
