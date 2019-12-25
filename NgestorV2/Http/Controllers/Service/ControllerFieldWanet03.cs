﻿using BotNgestor.Http;
using BotWanet03FieldService.Repository;
using DataBase;
using NgestorFieldServiceTools.App;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NgestorFieldServiceTools.Http.Controllers.Service
{
    class ControllerFieldWanet03
    {
        Wanet03StartAutoImport wanet03StartAutoImport =
           new Wanet03StartAutoImport(
               Config.id_user_ngestor,
               Config.id_operadora_servidor,
               Config.id_dominio_ngestor,
               Config.ngestor_url_server               
               );
        public static string statusNgestor = "Aguardando...";
        public async Task<string> execute()
        {
            string servicos = await wanet03StartAutoImport.start();

            if (await importNgestor(servicos, "field"))
            {
                insertStatusAutoImport(true, Wanet03Manager.totalServicos);
                statusNgestor = "Importação Concluida";
            }
            else
            {
                insertStatusAutoImport(false, 0);
            }

            Thread thread = new Thread(() =>
            {
                Thread.Sleep(1000 * 60 * wanet03StartAutoImport.timer);
#pragma warning disable CS4014 // Como esta chamada não é aguardada, a execução do método atual continua antes da conclusão da chamada. Considere aplicar o operador 'await' ao resultado da chamada.
                execute();
#pragma warning restore CS4014 // Como esta chamada não é aguardada, a execução do método atual continua antes da conclusão da chamada. Considere aplicar o operador 'await' ao resultado da chamada.
            })
            { IsBackground = true };
            thread.Start();

            return servicos;
        }

        public async Task<bool> importNgestor(string services, string sistema)
        {
            Importacao importacao = new Importacao();
            return await importacao.servicos(Config.token_negestor, Config.ngestor_url_server, services, sistema);
        }

        private void insertStatusAutoImport(bool isSucess, int countService)
        {
            if (isSucess)
            {
                DbMain database = new DbMain("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "Manager");
                database.ExecuteSqlCommand($"INSERT INTO ngestor_importacoes_automatica (id_dominio_ngestor, id_user, id_status, total_os, id_sistemas_tipo, data_importacao) VALUES({Config.id_dominio_ngestor}, {Config.id_user_ngestor}, 1, {countService}, {Config.id_operadora_servidor}, '{DateTime.Now.ToString()}')");
            }
            else
            {
                DbMain database = new DbMain("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "Manager");
                database.ExecuteSqlCommand($"INSERT INTO ngestor_importacoes_automatica (id_dominio_ngestor, id_user, id_status, total_os, id_sistemas_tipo, data_importacao) VALUES({Config.id_dominio_ngestor}, {Config.id_user_ngestor}, 2, {countService}, {Config.id_operadora_servidor}, '{DateTime.Now.ToString()}')");
            }
        }
    }
}
