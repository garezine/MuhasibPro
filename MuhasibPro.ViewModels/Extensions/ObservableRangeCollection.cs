using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MuhasibPro.ViewModels.Extensions
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public ObservableRangeCollection() : base() { }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        public ObservableRangeCollection(List<T> list) : base(list) { }

        /// <summary>
        /// Bildirimleri (CollectionChanged ve PropertyChanged) geçici olarak durdurur.
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Bildirimleri (CollectionChanged ve PropertyChanged) geçici olarak durdurur.
        /// </summary>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Koleksiyona bir öğe listesini performanslı bir şekilde ekler.
        /// İşlem sonunda tek bir 'Reset' bildirimi gönderilir.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (!items.Any()) return; // Eklenecek bir şey yoksa çık

            _suppressNotification = true;

            // Temel 'Items' listesine (protected) doğrudan ekleme yap
            foreach (var item in items)
            {
                Items.Add(item);
            }

            _suppressNotification = false;

            // Tüm eklemeler bittikten sonra, UI'a tek bir bildirim gönder.
            // "Count" ve "Item[]" (indexer) özelliklerinin değiştiğini bildir.
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            // Koleksiyonun tamamen değiştiğini (Reset) bildir.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Koleksiyondaki tüm öğeleri temizler ve yerlerine yeni listeyi ekler.
        /// İşlem sonunda tek bir 'Reset' bildirimi gönderilir.
        /// </summary>
        public void ReplaceRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            _suppressNotification = true;

            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            _suppressNotification = false;

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void RemoveRangeAnimated(IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove == null) throw new ArgumentNullException(nameof(itemsToRemove));

            var itemsToRemoveList = itemsToRemove.ToList();
            if (!itemsToRemoveList.Any()) return;

            // Bildirimleri geçici olarak durdur, çünkü temel koleksiyonu
            // manuel olarak biz değiştireceğiz.
            _suppressNotification = true;

            // Gerçekte kaldırılan öğelerin listesini tut
            var removedItems = new List<T>();
            // Hızlı arama için HashSet
            var setToRemove = new HashSet<T>(itemsToRemoveList);

            // Koleksiyonda gezinirken kaldıramayız, bu yüzden
            // 'Items' listesinde (protected) tersten gezinmek en güvenlisidir.
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (setToRemove.Contains(item))
                {
                    // Temel koleksiyondan kaldır
                    Items.RemoveAt(i);                    
                    // Kaldırılanlar listesine (doğru sırayla) ekle
                    removedItems.Add(item);
                }
            }

            _suppressNotification = false;

            if (removedItems.Count > 0)
            {
                // UI'ı bilgilendir
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

                // BURASI KRİTİK:
                // UI'a "Bu spesifik öğeleri kaldırdım" diyoruz.
                // 'Reset' demiyoruz. 'removedItems' listesini gönderiyoruz.
                // Not: 'removedItems' listesi şu an ters sırada,
                // ancak 'Remove' bildirimi için bu genellikle sorun olmaz.
                // Sorun olursa 'removedItems.Reverse()' yapılabilir.
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removedItems // Kaldırılan öğelerin listesi
                ));
            }
        }
        /// <summary>
        /// Belirtilen koşulu sağlayan tüm öğeleri koleksiyondan kaldırır.
        /// İşlem sonunda tek bir 'Reset' bildirimi gönderilir.
        /// </summary>
        /// 
        public void RemoveAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            _suppressNotification = true;

            var itemsToRemove = Items.Where(item => match(item)).ToList();
            bool removed = false;

            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
                removed = true;
            }

            _suppressNotification = false;

            if (removed)
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Koleksiyondaki öğeleri yerinde (in-place) sıralar.
        /// İşlem sonunda tek bir 'Reset' bildirimi gönderilir.
        /// </summary>
        public void SortInPlace(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));

            _suppressNotification = true;

            // ObservableCollection içindeki 'Items' listesine (protected IList<T>)
            // erişiyoruz ve onu List<T>'ye cast edip sıralıyoruz.
            var list = Items as List<T>;
            if (list != null)
            {
                list.Sort(comparison);
            }
            else
            {
                // Alternatif (daha yavaş) yol
                var sorted = Items.OrderBy(x => x, new ComparisonComparer<T>(comparison)).ToList();
                Items.Clear();
                foreach (var item in sorted) Items.Add(item);
            }

            _suppressNotification = false;

            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>
    /// SortInPlace için yardımcı IComparer sınıfı
    /// </summary>
    internal class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;
        public ComparisonComparer(Comparison<T> comparison) => _comparison = comparison;
        public int Compare(T x, T y) => _comparison(x, y);
    }
}
