# NPC Diyalog Dosyalari

## Klasor yapisi

```
{ad_soyad}/
  insan.json      # beats + fallback
  vampir.json     # banko vampir (her_zaman)
  meta.json       # guvenilirlik: normal | unreliable
_shared/
  templates/      # cinayet tanigi, yalan, red
  generic_smalltalk.json
  vampir_modifiers.json
```

## Beat kosullari

| Alan | Anlam |
|------|--------|
| `max_talk_count` | Ilk konusma icin 0 |
| `min_talk_count` | Onceki konusma sayisi |
| `min_kalinan_gun` | Misafirin kaldigi gece sayisi |
| `requires_flag` | FactJournal global flag |

## Oncelik (runtime)

1. Bekleyen tanik ifadesi (cinayet sonrasi)
2. Intro beat (intro_1, intro_2, intro_3)
3. Generic smalltalk fallback

## Template slotlari

- `{victim_name}`, `{victim_room}`, `{subject_name}`

Guvenilirlik: insan cogu dogru, vampir cogu yalan/reddeder, dusuk itibar dogrulugu dusurur.
