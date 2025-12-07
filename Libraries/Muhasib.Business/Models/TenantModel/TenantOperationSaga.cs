using Microsoft.Extensions.Logging;

namespace Muhasib.Business.Models.TenantModel
{
    public class TenantOperationSaga
    {
        private readonly ILogger _logger;
        private readonly List<SagaStep> _executedSteps = new List<SagaStep>();
        private readonly object _lock = new object();

        public TenantOperationSaga(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<T> ExecuteStepAsync<T>(
            string stepName,
            Func<Task<T>> action,
            Func<T, Task> compensate)
        {
            T result = default;

            try
            {
                _logger.LogInformation("Saga Step başlatıldı: {StepName}", stepName);

                result = await action();

                // ✅ KRİTİK: Step başarılı olduğunda HEMEN kaydet
                lock (_lock)
                {
                    _executedSteps.Add(new SagaStep
                    {
                        StepName = stepName,
                        Result = result,
                        CompensateAction = compensate != null
                            ? async () => await compensate(result)
                            : null
                    });
                }

                _logger.LogInformation("Saga Step tamamlandı: {StepName}", stepName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga Step başarısız: {StepName}", stepName);

                // Step başarısız oldu, compensate listesine ekleme
                // Çünkü zaten rollback yapılacak
                throw;
            }
        }

        public async Task CompensateAllAsync()
        {
            List<SagaStep> stepsToCompensate;

            lock (_lock)
            {
                stepsToCompensate = new List<SagaStep>(_executedSteps);
            }

            _logger.LogWarning("Toplam {Count} adım rollback edilecek", stepsToCompensate.Count);

            // Ters sırada rollback yap (LIFO - Last In First Out)
            for (int i = stepsToCompensate.Count - 1; i >= 0; i--)
            {
                var step = stepsToCompensate[i];

                if (step.CompensateAction != null)
                {
                    try
                    {
                        _logger.LogInformation("[{Index}/{Total}] Compensating: {StepName}",
                    stepsToCompensate.Count - i,
                    stepsToCompensate.Count,
                    step.StepName);


                        await step.CompensateAction();

                        _logger.LogInformation("[{Index}/{Total}] Compensated: {StepName}",
                     stepsToCompensate.Count - i,
                     stepsToCompensate.Count,
                     step.StepName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                   ex,
                   "[{Index}/{Total}] Compensation başarısız: {StepName} - Manuel müdahale gerekli!",
                   stepsToCompensate.Count - i,
                   stepsToCompensate.Count,
                   step.StepName);

                        // Diğer compensation'ları denemeye devam et
                    }
                }
                else
                {
                    _logger.LogInformation("[{Index}/{Total}] Compensate yok (atlandı): {StepName}",
               stepsToCompensate.Count - i,
               stepsToCompensate.Count,
               step.StepName);
                }
            }
        }

        private class SagaStep
        {
            public string StepName { get; set; }
            public object Result { get; set; }
            public Func<Task> CompensateAction { get; set; }
        }
    }

}
