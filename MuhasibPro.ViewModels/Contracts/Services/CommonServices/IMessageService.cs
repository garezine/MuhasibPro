#region copyright
// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
#endregion


#region copyright
// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
#endregion

namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices
{
    public interface IMessageService
    {
        void Subscribe<TSender>(object target, Action<TSender, string, object> action) where TSender : class;
        void Subscribe<TSender, TArgs>(object target, Action<TSender, string, TArgs> action) where TSender : class;

        // Unsubscribe metodları
        void Unsubscribe<TSender>(object target) where TSender : class;
        void Unsubscribe<TSender, TArgs>(object target) where TSender : class;
        void Unsubscribe(object target);

        // Send metodları - orijinal sync versiyonlar
        void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;

        // WinUI 3 için async versiyonlar
        Task SendAsync<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
        void RegisterContext(int contextId, IContextService contextService);
        void UnregisterContext(int contextId);
    }
}
