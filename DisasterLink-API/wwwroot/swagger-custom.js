// Script para personalizar a interface do Swagger
(function() {
    window.addEventListener('DOMContentLoaded', function() {
        setTimeout(function() {
            console.log("Executando customização do Swagger - v6 (cache busting e modais de atalho)");
            
            const infoContainer = document.querySelector('.swagger-ui .info');
            
            if (infoContainer) {
                // Remover a URL do swagger.json de forma mais robusta
                const linkElement = infoContainer.querySelector('a[href*="swagger.json"]');
                if (linkElement) {
                    const parentPre = linkElement.closest('pre');
                    if (parentPre) {
                        parentPre.style.display = 'none';
                        console.log("URL do swagger.json (e seu container <pre>) ocultada.");
                    } else {
                        linkElement.style.display = 'none'; // Fallback
                        console.log("URL do swagger.json (apenas o link) ocultada.");
                    }
                }

                adicionarLogoAoTitulo(infoContainer, 'DisasterLink-Systems-Logo.png');
                adicionarInformacoesAdicionais(infoContainer);
                ocultarBadgeOAS(infoContainer);

            } else {
                console.error("Container .swagger-ui .info não encontrado.");
            }
            
            adicionarInstrucoesAutenticacao();
            setupEndpointLockListeners(); // Configura listeners para os cadeados dos endpoints

        }, 2500); // Aumentado um pouco mais para garantir que tudo carregue
    });
    
    function adicionarLogoAoTitulo(infoContainer, nomeImagem) {
        const titleElement = infoContainer.querySelector('.title');

        if (titleElement && nomeImagem) {
            let logo = titleElement.querySelector('.api-logo');
            const logoSrc = `/${nomeImagem}?v=${new Date().getTime()}`; // Cache busting
            if (!logo) { 
                logo = document.createElement('img');
                logo.alt = 'DisasterLink API Logo';
                logo.className = 'api-logo';
                titleElement.insertBefore(logo, titleElement.firstChild);
                console.log(`Logo adicionado ANTES do título com src: ${logoSrc}`);
            } else {
                console.log(`Logo ATUALIZADO com src: ${logoSrc}`);
            }
            logo.src = logoSrc; // Define ou atualiza o src com cache busting
        } else {
            console.error("Elemento .title ou nome da imagem não fornecido para adicionarLogoAoTitulo.");
        }
    }

    function ocultarBadgeOAS(infoContainer) {
        const oasBadge = infoContainer.querySelector('.title > .version-stamp, .title > small.version'); 
        const oasBadgeAlternative = infoContainer.querySelector('.title span > pre.version');


        if (oasBadge) {
            oasBadge.style.display = 'none';
            console.log("Badge OAS (version-stamp) ocultado.");
        } else if (oasBadgeAlternative) {
             oasBadgeAlternative.style.display = 'none';
            console.log("Badge OAS (pre.version) ocultado.");
        }
        else {
            const allSpansInTitle = infoContainer.querySelectorAll('.title span');
            allSpansInTitle.forEach(span => {
                if (span.textContent.includes('OAS')) {
                    span.style.display = 'none';
                    console.log(`Badge OAS (genérico: "${span.textContent}") ocultado.`);
                }
            });
        }
    }

    function adicionarInformacoesAdicionais(infoContainer) { 
        let descElement = infoContainer.querySelector('.api-description');
        if (!descElement) { 
            descElement = document.createElement('p');
            descElement.className = 'api-description';
            const versionElement = infoContainer.querySelector('.info .version-pragma'); 
            if (versionElement) {
                versionElement.parentNode.insertBefore(descElement, versionElement.nextSibling);
            } else {
                infoContainer.appendChild(descElement);
            }
        }
        descElement.textContent = 'Bem-vindo à API DisasterLink! Esta é a interface central da API para a solução DisasterLink Systems, um ecossistema colaborativo projetado para mapear, monitorar e responder rapidamente a desastres naturais. Através desta API, aplicativos móveis e plataformas administrativas podem gerenciar ocorrências (relatos de cidadãos), disparar alertas, organizar campanhas de ajuda e muito mais, visando unir cidadãos, autoridades e tecnologia para uma resposta eficiente a emergências.';
        
        let linksContainer = infoContainer.querySelector('.api-links');
        if (!linksContainer) { 
            linksContainer = document.createElement('div');
            linksContainer.className = 'api-links';
            descElement.parentNode.insertBefore(linksContainer, descElement.nextSibling);

        }
        linksContainer.innerHTML = ''; 

            const linkData = [
            { text: 'Repositorio DisasterLink Systems', url: 'https://github.com/MacaulyV/DisasterLink-Systems' },
            { text: 'Contato', url: 'Macaulyv@gmail.com' },
                { text: 'Licença MIT', url: 'https://opensource.org/licenses/MIT' }
            ];
            
            linkData.forEach((ld, idx) => {
                const a = document.createElement('a');
                a.href = ld.url;
                a.target = '_blank';
                a.textContent = ld.text;
            linksContainer.appendChild(a);
                if (idx < linkData.length - 1) {
                    const sep = document.createTextNode(' | ');
                linksContainer.appendChild(sep);
            }
        });
        console.log("Informações adicionais (descrição e links) processadas.");
    }
    
    function adicionarInstrucoesAutenticacao() {
        const checkAuthButton = setInterval(() => {
            const authWrapper = document.querySelector('.swagger-ui .auth-wrapper');
            const authButton = authWrapper ? authWrapper.querySelector('.authorize') : null;

            if (authButton && authWrapper) {
                clearInterval(checkAuthButton);
                
                let authInstructions = authWrapper.querySelector('.auth-instructions');
                if (!authInstructions) {
                    authInstructions = document.createElement('div');
                    authInstructions.className = 'auth-instructions';
                    authWrapper.appendChild(authInstructions);
                }
                
                authInstructions.style.marginLeft = '15px';
                authInstructions.style.padding = '10px 15px';
                authInstructions.style.backgroundColor = '#eaf5fd'; 
                authInstructions.style.borderLeft = '5px solid #3498db';
                authInstructions.style.borderRadius = '0 5px 5px 0';
                authInstructions.style.display = 'inline-block'; 
                authInstructions.style.verticalAlign = 'middle';

                authInstructions.innerHTML = '<p style="margin:0; font-size: 14px; color: #2c3e50;">👉 <strong>Autenticação:</strong><br>1. Faça login como <strong>Admin</strong> na API para obter seu token.<br>2. Clique em <strong>Authorize</strong> e cole o token no formato <strong><code>Bearer seu-token-aqui</code></strong>.</p>';
                
                authButton.style.backgroundColor = '#145db1'; 
                authButton.style.color = 'white';
                authButton.style.border = 'none';
                authButton.style.borderRadius = '5px';
                authButton.style.padding = '10px 15px'; 
                authButton.style.fontWeight = 'bold';
                authButton.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
                authButton.textContent = '🔑 Autorizar'; // Alterado para Português

                console.log("Instruções de autenticação atualizadas.");

                // Personalizar o modal de autorização
                authButton.addEventListener('click', () => {
                    setTimeout(personalizarModalAuth, 100); // Pequeno delay para garantir que o modal esteja no DOM
                });

            }
        }, 800);
    }

    function personalizarModalAuth() {
        const modal = document.querySelector('.swagger-ui .dialog-ux .modal-ux');
        if (!modal) {
            console.warn("Modal de autorização não encontrado para personalização.");
            return;
        }

        // Alterar título do Modal
        const modalTitle = modal.querySelector('.modal-ux-header h3');
        if (modalTitle) {
            modalTitle.textContent = 'Autorização da API';
            console.log("Título do modal de autorização alterado.");
        }

        // Alterar texto do botão "Authorize" dentro do modal
        const modalAuthButton = modal.querySelector('.modal-ux-content .authorize.button');
        if (modalAuthButton) {
            modalAuthButton.textContent = 'Autorizar Sessão';
            console.log("Botão 'Authorize' do modal alterado.");
        }

        // Alterar texto do botão "Close"
        const modalCloseButton = modal.querySelector('.modal-ux-content .btn-done');
        if (modalCloseButton) {
            modalCloseButton.textContent = 'Fechar';
            console.log("Botão 'Close' do modal alterado.");
        }

        // Alterar texto do botão "Logout" (se existir e estiver visível)
        const modalLogoutButton = modal.querySelector('.modal-ux-content .authorize.button.locked');
         if (modalLogoutButton && modalLogoutButton.textContent.toLowerCase().includes('logout')) {
            modalLogoutButton.textContent = 'Revogar Autorização';
            console.log("Botão 'Logout' do modal alterado.");
        }


        // Melhorar a descrição do token
        const tokenDescription = modal.querySelector('.modal-ux-content .auth-description');
        if (tokenDescription) {
            tokenDescription.innerHTML = `
                <p>Para autorizar suas requisições na API, insira seu token de acesso no campo abaixo.</p>
                <p>O token é obtido após o login. Utilize o formato: <strong><code>Bearer &lt;SEU_TOKEN_AQUI&gt;</code></strong></p>
                <p>Substitua <code>&lt;SEU_TOKEN_AQUI&gt;</code> pelo seu token. Não se esqueça do prefixo "<strong>Bearer </strong>" seguido de um espaço.</p>
                <p>Exemplo prático: <code>Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</code></p>
            `;
            console.log("Descrição do token no modal principal atualizada.");
        }
        
        // Adicionar um placeholder mais amigável ao campo de valor do token
        const tokenInput = modal.querySelector('.modal-ux-content input[type="text"]');
        if (tokenInput) {
            tokenInput.placeholder = 'Cole seu token Bearer aqui';
            console.log("Placeholder do campo de token atualizado.");
        }
    }

    function setupEndpointLockListeners() {
        const operationsObserver = new MutationObserver((mutationsList, observer) => {
            for (const mutation of mutationsList) {
                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === 1) {
                            // Lógica para os modais de atalho de cadeado
                            if (node.matches('.opblock-summary-control button[aria-label*="authorization"], .opblock-control-arrow button[aria-label*="authorization"] ') || node.querySelector('.opblock-summary-control button[aria-label*="authorization"], .opblock-control-arrow button[aria-label*="authorization"] ')) {
                                const buttons = node.matches('button') ? [node] : node.querySelectorAll('.opblock-summary-control button[aria-label*="authorization"], .opblock-control-arrow button[aria-label*="authorization"] ');
                                buttons.forEach(btn => {
                                    if (!btn.dataset.listenerAttached) {
                                        btn.addEventListener('click', handleEndpointAuthClick);
                                        btn.dataset.listenerAttached = 'true';
                                        console.log("Listener adicionado ao botão de cadeado do endpoint.");
                                    }
                                });
                            }
                            // Lógica para destacar endpoints que requerem autenticação
                            highlightSecureEndpoints(node.matches('.opblock') ? node : node.querySelector('.opblock'));
                        }
                    });
                }
            }
             // Re-checar todos após mutações em lote
            document.querySelectorAll('.swagger-ui .opblock').forEach(opblock => highlightSecureEndpoints(opblock));
        });

        const operationsContainer = document.querySelector('.swagger-ui .operations');
        if (operationsContainer) {
            operationsObserver.observe(operationsContainer, { childList: true, subtree: true });
            // Adicionar listeners aos botões já existentes
            document.querySelectorAll('.opblock-summary-control button[aria-label*="authorization"], .opblock-control-arrow button[aria-label*="authorization"] ').forEach(btn => {
                if (!btn.dataset.listenerAttached) {
                    btn.addEventListener('click', handleEndpointAuthClick);
                    btn.dataset.listenerAttached = 'true';
                }
            });
            console.log("Listeners para cadeados de endpoint configurados e MutationObserver iniciado.");
            // Destacar endpoints seguros já existentes na carga inicial
            document.querySelectorAll('.swagger-ui .opblock').forEach(opblock => highlightSecureEndpoints(opblock));
        } else {
            console.warn("Container de operações não encontrado para configurar listeners de cadeado de endpoint. Tentando novamente em breve.");
            setTimeout(setupEndpointLockListeners, 1500);
        }
    }

    function handleEndpointAuthClick() {
        // O modal de autorização de endpoint é adicionado dinamicamente e pode levar um momento.
        // Usamos um pequeno timeout e um seletor mais específico.
        console.log("Cadeado de endpoint clicado, aguardando modal de atalho.");
        setTimeout(() => {
            // O modal de atalho do endpoint não é o mesmo que o modal principal.
            // Ele geralmente aparece como um popover ou um modal menor diretamente associado ao endpoint.
            // O seletor precisa ser ajustado conforme a estrutura real do Swagger UI para esses modais.
            // Tentativa comum: o último modal adicionado que não é o de diálogo principal.
            const endpointAuthModals = document.querySelectorAll('.swagger-ui .dialog-ux .modal-ux, .swagger-ui .auth-popup');
            let targetModal = null;
            if (endpointAuthModals.length > 0) {
                 // Seleciona o último modal que não seja o principal (se o principal já estiver aberto)
                for (let i = endpointAuthModals.length - 1; i >= 0; i--) {
                    if (!endpointAuthModals[i].querySelector('.modal-ux-header h3') || 
                        (endpointAuthModals[i].querySelector('.modal-ux-header h3') && endpointAuthModals[i].querySelector('.modal-ux-header h3').textContent !== 'Autorização da API')) {
                        targetModal = endpointAuthModals[i];
                        break;
                    }
                }
                 if (!targetModal && endpointAuthModals.length > 0) targetModal = endpointAuthModals[endpointAuthModals.length -1]; // Fallback para o último se a lógica acima falhar
            }

            if (targetModal) {
                console.log("Modal de atalho de autorização encontrado, personalizando...");
                personalizarModalDeAtalhoAuth(targetModal);
            } else {
                console.warn("Modal de atalho de autorização não encontrado após clique no cadeado do endpoint.");
            }
        }, 200); // Ajuste o delay se necessário
    }

    function personalizarModalDeAtalhoAuth(modal) {
        if (!modal) return;

        console.log("Personalizando modal de atalho: ", modal);

        // Título (se houver, geralmente não há um h3 proeminente como no principal)
        const schemeTitle = modal.querySelector('.auth-scheme-name');
        if (schemeTitle && schemeTitle.textContent.toLowerCase().includes('customtoken')) {
            // Não vamos alterar o nome do esquema "CustomToken (apiKey)" pois é técnico,
            // mas podemos adicionar um subtítulo ou instrução se necessário.
            console.log("Nome do esquema no modal de atalho: ", schemeTitle.textContent);
        }

        // Campo de valor
        const valueInput = modal.querySelector('input[type="text"]');
        if (valueInput) {
            valueInput.placeholder = 'Bearer <seu-token-aqui>';
            console.log("Placeholder do input no modal de atalho alterado.");
        }

        // Botões
        const authorizeButton = modal.querySelector('.btn.authorize, button.authorize');
        if (authorizeButton) {
            if (authorizeButton.textContent.toLowerCase().includes('logout')) {
                authorizeButton.textContent = 'Revogar'; 
            } else {
                authorizeButton.textContent = 'Autorizar';
            }
            console.log("Botão 'Authorize/Logout' do modal de atalho alterado.");
        }

        const closeButton = modal.querySelector('.btn.btn-done, button.btn-done, .btn-close, button.close'); // Tentar vários seletores comuns
        if (closeButton) {
            closeButton.textContent = 'Fechar';
            console.log("Botão 'Close' do modal de atalho alterado.");
        }
    }

    function highlightSecureEndpoints(opblockElement) {
        if (!opblockElement || opblockElement.dataset.authHighlightApplied) return;

        const lockIcon = opblockElement.querySelector('.opblock-summary-control button[aria-label*="authorization"], svg[aria-label*="authorization"]');
        
        if (lockIcon) {
            let summaryElement = opblockElement.querySelector('.opblock-summary-path');
            if (!summaryElement) summaryElement = opblockElement.querySelector('.opblock-summary-description'); // Fallback
            
            if (summaryElement) {
                let authBadge = summaryElement.querySelector('.auth-required-badge');
                if (!authBadge) {
                    authBadge = document.createElement('span');
                    authBadge.className = 'auth-required-badge';
                    authBadge.textContent = '🛡️ Autenticação Obrigatória';
                    // authBadge.title = 'Este endpoint requer autenticação';
                    
                    // Tenta inserir após o path, mas antes de outros elementos como a descrição
                    const descriptionElement = opblockElement.querySelector('.opblock-summary-description');
                    if (descriptionElement && summaryElement.contains(descriptionElement)) {
                        summaryElement.insertBefore(authBadge, descriptionElement);
                    } else {
                         summaryElement.appendChild(authBadge); // Adiciona ao final se não encontrar descrição
                    }
                    opblockElement.dataset.authHighlightApplied = 'true';
                    console.log("Badge de autenticação adicionado ao endpoint: ", summaryElement.textContent);
                }
            } else {
                console.warn("Elemento de resumo não encontrado para adicionar badge de autenticação em: ", opblockElement);
            }
        } 
    }

})(); 