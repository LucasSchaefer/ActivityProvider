# Activity Provider: Tradução Colaborativa

Tradução Colaborativa é um módulo Activity Provider voltado para a arquitetura Inven!RA, que permite a tradução colaborativa de documentos e textos, e também o peer review das traduções, sejam eles traduzidos de forma comercial ou com finalidade acadêmica. O objetivo deste módulo é permitir com que plataformas de tradução possam implementar a arquitetura Inven!RA e abstrair parte da gestão de correções de tradução e a tarefa de gerir os analytics relacionados às mesmas.
Pequenos trechos de texto ou frases serão atribuídos aos tradutores, que deverão traduzi-los da forma mais natural possível. A aplicação fornecerá ao tradutor uma visualização do texto original (para contexto) com o trecho a traduzir em destaque, e também um editor de texto onde o tradutor deverá inserir o texto traduzido. O revisor terá acesso à mesma interface, onde poderá corrigir as traduções e indicar os acertos e erros.

Autor: Lucas Schaefer

## Endpoints
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
### Obtém o status do processo para o Actor (user autenticado)
GET /status