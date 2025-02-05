﻿"use client"
import Image from "next/image";
import {Card, CardActionArea, CardContent, Chip, Grid2, Typography} from "@mui/material";
import {PokemonDto} from "@shared/types";
import {capitalizeFirstLetter, LAST_VISITED_FRAGMENT, padWithLeadingZeros} from "@shared/utils";
import {useRouter} from "next/navigation";
import {useLastVisitedFragment} from "@shared/hooks";

interface PokemonCardProps {
    id: number
    speciesName: string
    baby: boolean
    legendary: boolean
    mythical: boolean
    pokemon: PokemonDto
}

export default function PokemonCard(props: PokemonCardProps) {
    const {id, speciesName, baby, legendary, mythical, pokemon} = props;
    const router = useRouter();
    useLastVisitedFragment()
    const officialArtworkFrontDefault = pokemon
        .images
        .find(media => media.file_name === `${pokemon.name}-official-artwork-front-default.png`)

    if (!officialArtworkFrontDefault) {
        throw new Error(`${pokemon.name}-official-artwork-front-default.png`);
    }

    const types = pokemon.types.map((type) => (
        <Grid2 key={type.type}>
            <Chip label={capitalizeFirstLetter(type.type)} variant={type.type as "outlined"}/>
        </Grid2>
    ));

    const isBaby = baby ? <Chip label={"Baby"} variant={"outlined"}/> : null
    const isLegendary = legendary ? <Chip label={"Legendary"} variant={"outlined"}/> : null
    const isMythical = mythical ? <Chip label={"Mythical"} variant={"outlined"}/> : null

    return <>
        <Card sx={{
            width: "280px",
        }}>
            <CardActionArea
                onClick={() => {
                    sessionStorage.setItem(LAST_VISITED_FRAGMENT, `${speciesName}`)
                    router.push(`/pokemon/${speciesName}`)
                }}
            >
                <Image
                    src={officialArtworkFrontDefault.src}
                    alt={pokemon.name}
                    width={200}
                    height={200}
                />
                <CardContent>
                    <Grid2 container
                           spacing={1}>
                        <Grid2 size={12}>
                            <Typography gutterBottom variant="h5" component="div">
                                {capitalizeFirstLetter(speciesName)}
                            </Typography>
                        </Grid2>
                        {types}
                        {isBaby}
                        {isLegendary}
                        {isMythical}
                        <Grid2 size={12}>
                            <Typography fontSize={12} color="text.secondary">
                                #{padWithLeadingZeros(id, 4)}
                            </Typography>
                        </Grid2>
                    </Grid2>
                </CardContent>
            </CardActionArea>
        </Card>
    </>
}