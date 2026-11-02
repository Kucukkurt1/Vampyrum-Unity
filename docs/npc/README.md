# NPC Listesi (Cursor'dan duzenle)

```
docs/npc/characters/
  nikolai_vasilev.json
  _SABLON.json
docs/npc/dialogue/
  nikolai_vasilev/
  _shared/
```

## Dosya adi

`ad_soyad.json` (kucuk harf, Turkce karakter yok). Icerideki `id` ayri sayi.

## Alanlar

| Alan | Aciklama |
|------|----------|
| **id** | Benzersiz sayi (oyun kodu) |
| **ad / soyad** | Karakter adi |
| **yas** | Yas |
| **cinsiyet** | `Kadin` veya `Erkek` |
| **model_id** | Gorsel model numarasi (0 = henuz yok) |
| **rol** | `misafir`, `ozel_npc`, `staff` |
| **vampir_atama** | `her_zaman`, `asla`, `rastgele` — JSON'da `is_vampir` YOK |
| **kalis_gunu** | `{ "min": 3, "max": 6 }` — spawn aninda rastgele kalis |
| **ucret_gunluk** | `{ "min": 2, "max": 4 }` — gunluk konaklama ucreti araligi |
| **maliyet_gunluk** | `{ "min": 1, "max": 3 }` — barindirma gideri araligi |
| **aktif** | Spawn havuzunda mi |

## vampir_atama (uc katman)

| Deger | Anlam |
|-------|--------|
| `her_zaman` | Banko vampir — her save/run'da vampir (az sayida hikaye karakteri) |
| `asla` | Banko insan — hicbir save'de vampir olamaz (staff, guvenilir yuz) |
| `rastgele` | Run basinda havuzdan secilir; geri kalan cast |

Garanti vampir **JSON bayragi degil** — atama kurali. Gercek bayrak runtime: `GuestProfile.IsVampire`.

## Ekonomi

Spawn aninda araliklardan tek deger roll edilir (run seed + karakter id):

- **Kabul:** `ucret_gunluk x planlanan_gun` pesin kasaya girer
- **Her gece:** `maliyet_gunluk` gider olarak duser
- **Erken olum:** Iade = `(planlanan - kalinan) x ucret_gunluk`

Ornek roll: 5 gun x 3 altin ucret, 2 altin/gun maliyet → +15 pesin, -10 maliyet (5 gece) = net +5

## Oda

JSON'da oda yok. Kabul aninda bos `RoomSlot` atanir (Unity sahnesinde anchor).

## Diyalog

```
docs/npc/dialogue/{ad_soyad}/
  insan.json    — ana agac (her karakter)
  vampir.json   — banko vampirler icin (her_zaman)
docs/npc/dialogue/_shared/
  vampir_modifiers.json
  tells.json
```

| Karakter tipi | Kaynak |
|---------------|--------|
| Banko vampir (`her_zaman`) | `vampir.json` |
| Banko insan (`asla`) | `insan.json` |
| Rastgele + bu run'da vampir | `insan.json` + `_shared/vampir_modifiers` |
| Rastgele + insan | `insan.json` |

## Yeni karakter

1. `_SABLON.json` kopyala → `yeni_ad_soyad.json`
2. `id` ve alanlari doldur
3. `npc_roster.json` listesine ekle
4. `docs/npc/dialogue/yeni_ad_soyad/insan.json` olustur
